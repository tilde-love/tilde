// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tilde.Core.Projects;

namespace Tilde.Core.Work
{
    /// <summary>
    /// Laborer represent a single process. 
    /// </summary>
    public class Laborer
    {
        public string Name { get; set; }

        public Uri ProjectUri { get; set; }
        
        [JsonIgnore] public Project Project { get; set; }

        public LaborRestartPolicy RestartPolicy { get; set; }

        public ILaborRunner Runner { get; set; }

        public int? ExitCode { get; private set; } 

        private CancellationTokenSource cancellationTokenSource;
        private Task<RunResult> task;
        private CancellationTokenSource linkedTokenSource;

        public void Run(CancellationToken cancellationToken)
        {
            Stop(cancellationToken); 
            
            cancellationTokenSource = new CancellationTokenSource();
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);
            
            task = Runner.Work(
                linkedTokenSource.Token
            );
        }

        public void Stop(CancellationToken cancellationToken)
        {
            Task<RunResult> runTask = task;
            task = null; 
            
            if (runTask == null)
            {
                return; 
            }

            if (runTask.IsCompleted == true)
            {
                ExitCode = runTask.Result.ExitCode;

                return; 
            }
            
            cancellationTokenSource.Cancel();
            
            runTask.Wait();

            ExitCode = runTask.Result.ExitCode;
        }

        public void Dispose()
        {
            Stop(CancellationToken.None); 
                
            linkedTokenSource.Dispose();
            cancellationTokenSource.Dispose();
        }
    }

    [JsonConverter(typeof(LaborRunnerConverter))]
    public interface ILaborRunner // : IDisposable
    {
        [JsonProperty("type")]
        string Type { get; }
        
        [JsonProperty("state")]
        LaborState State { get; set; }

        Task<RunResult> Work(CancellationToken cancellationToken);
    }

    public class RunResult
    {
        public int? ExitCode { get; set; }
        
        public string Message { get; set; }
    }

    public class LaborRunnerConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<string, Type> converters = new ConcurrentDictionary<string, Type>(); 
            
        public static void RegisterConverter<TRunner>() where TRunner : ILaborRunner
        {
            converters[typeof(TRunner).FullName] = typeof(TRunner); 
        }

        /// <inheritdoc />
        public override bool CanWrite => false; 

        public override bool CanConvert(Type objectType)
        {
            return converters.Values.Aggregate(false, (current, type) => current | type.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            
            string type = item["type"].Value<string>(); 
            
            if (converters.TryGetValue(type, out Type converterType))
            {
                return item.ToObject(converterType, serializer); 
            }

            return existingValue; 
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
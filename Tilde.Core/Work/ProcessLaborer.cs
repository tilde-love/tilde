// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Core.Work
{
    public class ProcessLaborer : ILaborRunner
    {
        [JsonProperty("arguments")]
        public string Arguments { get; set; }

        [JsonProperty("executable")]
        public string Executable { get; set; }

        [JsonProperty("environment")]
        public Dictionary<string, string> Environment { get; set; }

        private readonly AsyncQueue standardInput = new AsyncQueue();

        [JsonProperty("working")]
        public string WorkingDirectory { get; set; }

        public ProcessLaborer()
        {
            State = LaborState.Created;
        }

        public void Send(string value)
        {
            standardInput.Enqueue(value); 
        }

        /// <inheritdoc />
        public async Task<(int? exitCode, string message)> Work(Project project, CancellationToken cancellationToken)
        {
            using (ProjectLog log = new ProjectLog(project, LogType.Runtime))
            {
                int? result = await ProcessAsyncHelper.RunProcessAsync(
                    Executable,
                    Arguments,
                    WorkingDirectory,
                    Environment,
                    log.Log,
                    log.Log,
                    standardInput,
                    s =>
                    {
                        Console.WriteLine($"Process state: {s}");
                        State = s;
                    },
                    10000,
                    cancellationToken
                );

                return (result, "Exited");
            }
        }

        [JsonProperty("state")]
        public LaborState State { get; set; }

        /// <inheritdoc />
        [JsonProperty("type")]
        public string Type => GetType().FullName;
    }
}
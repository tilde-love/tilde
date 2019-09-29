// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        private Task task;
        private CancellationTokenSource linkedTokenSource;

        public void Run(CancellationToken cancellationToken)
        {
            Stop(cancellationToken); 
            
            cancellationTokenSource = new CancellationTokenSource();
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellationTokenSource.Token);

            task = Runner
                .Work(Project, linkedTokenSource.Token)
                .ContinueWith(t => { ExitCode = t.Result.exitCode; });
        }

        public void Stop(CancellationToken cancellationToken)
        {
            Task runTask = task;
            task = null; 
            
            if (runTask == null)
            {
                return; 
            }

            if (runTask.IsCompleted == true)
            {
                return; 
            }
            
            cancellationTokenSource.Cancel();

            try
            {
                runTask.Wait();
            }
            catch (TaskCanceledException)
            {
                // eat this exception.
                return;
            }
            catch (AggregateException aex)
            {
                foreach (Exception ex in aex.Flatten().InnerExceptions)
                {
                    switch (ex)
                    {
                        case TaskCanceledException cex:
                            // eat this exception.
                            break;
                        default:
                            throw;
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop(CancellationToken.None); 
                
            linkedTokenSource.Dispose();
            cancellationTokenSource.Dispose();
        }
    }
}
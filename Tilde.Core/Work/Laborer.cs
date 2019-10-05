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
        public Uri Name { get; set; }

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

            Runner.StateChanged += OnStateChanged;
            
            task = Runner
                .Work(Project, linkedTokenSource.Token)
                .ContinueWith(t => {
                    try
                    {
                        Runner.StateChanged -= OnStateChanged;
                        
                        ExitCode = t.Result.exitCode;
                        
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
                                    Console.WriteLine(ex.ToString());
                                    throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        
                        throw; 
                    }
                    finally
                    {
                        StateChanged?.Invoke(this);
                    }
                });
        }

        private void OnStateChanged(ILaborRunner laborRunner, LaborState laborState)
        {
            StateChanged?.Invoke(this);
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

        public event Action<Laborer> StateChanged;
    }
}
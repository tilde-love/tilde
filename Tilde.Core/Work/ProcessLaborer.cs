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
        //private ProjectLog log;
        //private Process process;
        //private Task<int?> process;

        [JsonProperty("arguments")]
        public string Arguments { get; set; }

        [JsonProperty("executable")]
        public string Executable { get; set; }

//        [JsonProperty("log")]
//        public string Log { get; set; }
        
        [JsonProperty("environment")]
        public Dictionary<string, string> Environment { get; set; }

//        [JsonIgnore]
//        public StreamReader StandardError => process?.StandardError ?? null;
//
        [JsonIgnore]
        public WrapperStreamWriter StandardInput => new WrapperStreamWriter();
//
//        [JsonIgnore]
//        public StreamReader StandardOutput => process?.StandardOutput ?? null;

        [JsonProperty("working")]
        public string WorkingDirectory { get; set; }

        public ProcessLaborer()
        {
            State = LaborState.Stopped;
        }
        
        /// <inheritdoc />
        public async Task<RunResult> Work(CancellationToken cancellationToken)
        {
            //Interlocked.Exchange(ref log, new ProjectLog(Log, LogType.Runtime))?.Dispose();
            Action<string> stdOut = s => { Console.Write(s); };

            State = LaborState.Running;
            
            int? result = await ProcessAsyncHelper.RunProcessAsync(
                Executable,
                Arguments,
                WorkingDirectory,
                Environment,
                stdOut,
                stdOut,
                StandardInput,
                1000,
                cancellationToken
            );

            if (result.HasValue == true && result.Value == 0)
            {
                State = LaborState.Exited;
            }
            else if (result.HasValue == true)
            {
                State = LaborState.ExitedWithCode;
            }
            else
            {
                State = LaborState.ExitedWithoutCode;
            }

            return new RunResult() {ExitCode = result, Message = "Did the process"};
//                
//
//            ProcessStartInfo startInfo = new ProcessStartInfo
//            {
//                FileName = Executable,
//                Arguments = Arguments,
//                UseShellExecute = false,
//                CreateNoWindow = true,
//                RedirectStandardOutput = true,
//                RedirectStandardError = true,
//                RedirectStandardInput = true,
//                WorkingDirectory = WorkingDirectory
//            };
//
//            foreach (var environmentVariable in Environment)
//            {
//                startInfo.Environment[environmentVariable.Key] = environmentVariable.Value; 
//            }
//            // startInfo.Environment["COREHOST_TRACE"] = "1"; 
//
//            process = new Process
//            {
//                StartInfo = startInfo,
//                EnableRaisingEvents = true
//            };
//
//            process.OutputDataReceived += ProcessOnOutputDataReceived;
//            process.ErrorDataReceived += ProcessOnErrorDataReceived;
//
//            if (process.Start() == false)
//            {
//                throw new Exception("Process failed to start.");
//            }
//
//            process.BeginOutputReadLine();
//            process.BeginErrorReadLine();
        }

        [JsonProperty("state")]
        public LaborState State { get; set; }

//        /// <inheritdoc />
//        public void Stop()
//        {
//            DisposeProcess(Interlocked.Exchange(ref process, null));
//
//            Interlocked.Exchange(ref log, null)?.Dispose();
//        }

        /// <inheritdoc />
        [JsonProperty("type")]
        public string Type => GetType().FullName;

//        private void DisposeProcess(Process proc)
//        {
//            if (proc == null)
//            {
//                return;
//            }
//
//            Debug.WriteLine("Shutting down script");
//            
//            proc.StandardInput.WriteLine(ModuleCommand.Exit.ToString());
//
//            if (proc.WaitForExit(10000) == false)
//            {
//                Debug.WriteLine("Shutdown failed killing process");
//                proc.Kill();
//            }
//
//            proc.OutputDataReceived -= ProcessOnOutputDataReceived;
//            proc.ErrorDataReceived -= ProcessOnErrorDataReceived;
//
//            proc?.Dispose();
//            Debug.WriteLine("Done!");
//        }

//        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            log.Log(e.Data);
//        }
//
//        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            log.Log(e.Data);
//        }
    }
}
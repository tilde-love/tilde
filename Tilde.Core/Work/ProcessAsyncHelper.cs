using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tilde.SharedTypes;

namespace Tilde.Core.Work
{
    internal static class ProcessAsyncHelper
    {
        public static async Task<int?> RunProcessAsync(
            string executable,
            string arguments,
            string workingDirectory, 
            Dictionary<string, string> environment,
            Action<string> stdOut,
            Action<string> stdError,
            AsyncQueue stdIn,
            Action<LaborState> updateState,
            int timeout, 
            CancellationToken cancellationToken)
        {
            int? result = null;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = executable;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                
                foreach (var environmentVariable in environment)
                {
                    process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value; 
                }

                Console.WriteLine($" $ {process.StartInfo.WorkingDirectory} {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                
                bool isStarted = process.Start();

                if (!isStarted)
                {
                    updateState(LaborState.Exited); 
                    
                    result = process.ExitCode;
                    
                    return result;
                }
                
                updateState(LaborState.Running); 
                
                // Create task to wait for process exit and closing all output streams
                Task processTask = Task.WhenAll(
                    WaitForExitAsync(process, updateState, timeout, cancellationToken),
                    WriteStreamWriterAsync(process, process.StandardInput, stdIn, cancellationToken),
                    ReadStreamReaderAsync(process, process.StandardOutput, stdOut, cancellationToken),
                    ReadStreamReaderAsync(process, process.StandardError, stdError, cancellationToken)
                );

                await processTask; 

                result = process.ExitCode;
            }

            return result;
        }

        private static async Task ReadStreamReaderAsync(
            Process p,
            StreamReader reader,
            Action<string> @out,
            CancellationToken ct)
        {
            char[] buffer = new char[1024 * 4];

            while (ct.IsCancellationRequested == false && p.HasExited == false)
            {
                int count = await reader.ReadAsync(buffer, 0, buffer.Length);

                if (count == 0)
                {
                    return;
                }

                @out.Invoke(new string(buffer, 0, count));
            }
        }

        private static async Task WriteStreamWriterAsync(
            Process process,
            StreamWriter processStandardInput,
            AsyncQueue stdIn,
            CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                string value = await stdIn.DequeueAsync(cancellationToken);

                if (value == null)
                {
                    continue; 
                }

                await processStandardInput.WriteAsync(value);
            }
        }

        private static async Task WaitForExitAsync(Process process, Action<LaborState> updateState, int timeout, CancellationToken cancellationToken)
        {
            var cancelSource = new CancellationTokenSource(); 
                
            void OnProcessOnExited(object sender, EventArgs args)
            {
                cancelSource.Cancel();
            }

            try
            {
                process.Exited += OnProcessOnExited;

                using (cancelSource)
                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancelSource.Token, cancellationToken))
                using (var cancellationTask = new CancellationTokenTaskSource<object>(linked.Token))
                {
                    try
                    {
                        await cancellationTask.Task;
                    }
                    catch (TaskCanceledException)
                    {
                    }

                    // Application cancelled the process.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // send the module EXIT command. 
                        process.StandardInput.WriteLine(ModuleCommand.Exit.ToString());

                        // closing the input should submit ctrl+c to the process when the input is read.
                        process.StandardInput.Close();

                        if (process.WaitForExit(timeout) == false)
                        {
                            process.Kill();
                        }

                        return;
                    }
                    else
                    {
                        // Process exited naturally.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("WAIT FOR EXIT ================================================");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("WAIT FOR EXIT ================================================");
                throw;
            }
            finally
            {
                updateState(LaborState.Exited); 
                
                process.Exited -= OnProcessOnExited;
            }
        }
    }
}
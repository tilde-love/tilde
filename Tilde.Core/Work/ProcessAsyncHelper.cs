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
            WrapperStreamWriter stdIn,
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
                
                async Task<bool> ReadStreamReaderAsync(Process p, StreamReader reader, Action<string> @out, CancellationToken ct)
                {
                    try
                    {
                        char[] buffer = new char[1024 * 4];

                        while (ct.IsCancellationRequested == false && p.HasExited == false)
                        {
                            int count = await reader.ReadAsync(buffer, 0, buffer.Length);
                            //int count = reader.Read(buffer, 0, buffer.Length);

                            if (count == 0)
                            {
                                return true;
                            }

                            @out.Invoke(new string(buffer, 0, count));
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("READ STREAM ================================================");
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("READ STREAM ================================================");
                        throw; 
                    }
                }
                
                try
                {
                    Console.WriteLine($" $ {process.StartInfo.WorkingDirectory} {process.StartInfo.FileName} {process.StartInfo.Arguments}");
                    
                    bool isStarted = process.Start();

                    if (!isStarted)
                    {
                        result = process.ExitCode;
                        
                        return result;
                    }

                    stdIn.Wrapped = process.StandardInput; 

                    // Create task to wait for process exit and closing all output streams
                    Task<bool[]> processTask = Task.WhenAll(
                        WaitForExitAsync(process, timeout, cancellationToken),
                        ReadStreamReaderAsync(process, process.StandardOutput, stdOut, cancellationToken),
                        ReadStreamReaderAsync(process, process.StandardError, stdError, cancellationToken)
                    );

                    bool cancelled = (await processTask)[0];
                    
                    result = process.ExitCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("PROCESS ================================================");
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("PROCESS ================================================");
                    throw; 
                }
                finally
                {
                    stdIn.Wrapped = null;
                }
            }

            return result;
        }
        
        private static async Task<bool> WaitForExitAsync(Process process, int timeout, CancellationToken cancellationToken)
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

                        if (process.WaitForExit(10000) == false)
                        {
                            process.Kill();
                        }

                        return true;
                    }
                    else
                    {
                        // Process exited naturally.
                        return false;
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
                process.Exited -= OnProcessOnExited;
            }
        }
    }
}
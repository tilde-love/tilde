// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Module
{
    public interface IRunnable
    {
        void Pause();

        void Play();
        
        Task Run(ModuleConnection connection, CancellationToken cancellationToken);
    }

    public enum ModuleCommand
    {
        Play,
        Pause,
        Exit
    }

    public class ModuleRunner
    {
        public static void Run(string host, string moduleName, IRunnable runnable)
        {
            CancellationTokenSource exitCancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) => { exitCancellationTokenSource.Cancel(); };

            Task runnableTask = null;

            try
            {
                ModuleConnection connection = new ModuleConnection(
                    new Uri(host, UriKind.RelativeOrAbsolute),
                    new Uri(moduleName, UriKind.RelativeOrAbsolute),
                    exitCancellationTokenSource.Token
                );
                
                runnableTask = RunnerTask(runnable, connection, exitCancellationTokenSource.Token);

                do
                {
                    string input = Console.ReadLine();

                    if (Enum.TryParse(input, out ModuleCommand command) == false)
                    {
                        throw new Exception("Unknown command");
                    }

                    switch (command)
                    {
                        case ModuleCommand.Play:
                            runnable.Play();
                            break;
                        case ModuleCommand.Pause:
                            runnable.Pause();
                            break;
                        case ModuleCommand.Exit:
                            return;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                try
                {
                    exitCancellationTokenSource.Cancel();

                    runnableTask?.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unhandled Exception");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static async Task RunnerTask(IRunnable runnable, ModuleConnection connection, CancellationToken cancellationToken)
        {
            await Task.Yield();

            try
            {
                await connection.StartAsync();

                await runnable.Run(connection, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await connection.StopAsync();

                await connection.DisposeAsync();
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Tilde.Core.Work;

namespace Tilde.Host
{
    public class BossService : IHostedService, IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource linkedCancellationTokenSource;
        private Task bossTask; 
        
        public Boss Boss { get; }

        public BossService(Boss boss)
        {
            Boss = boss; 
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationTokenSource = new CancellationTokenSource();

            linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
                
            bossTask = Boss.StartAsync(linkedCancellationTokenSource.Token);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationTokenSource.Cancel();

                if (bossTask.IsCanceled)
                {
                    Console.WriteLine("Boss task canceled");
                }
                else if (bossTask.IsCompleted)
                {
                    Console.WriteLine("Boss task completed");
                }
                else
                {
                    bossTask.Wait(); // cancellationToken
                }
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
                            Console.WriteLine("************************************************************************************");
                            Console.WriteLine(ex.ToString());
                            Console.WriteLine("************************************************************************************");
                            throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("************************************************************************************");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("************************************************************************************");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            cancellationTokenSource?.Dispose();
            linkedCancellationTokenSource?.Dispose();
            bossTask?.Dispose();
        }
    }
}
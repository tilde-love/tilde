using System;
using System.Threading;
using System.Threading.Tasks;
using Zero.Module;

namespace App
{
    public class Module : IRunnable
    {
        public void Pause()
        {
        }

        public void Play()
        {
        }

        // Run the actual module code 
        public async Task Run(ModuleConnection moduleConnection, CancellationToken cancellationToken)
        {
            Console.WriteLine("Module start");

            int i = 0;
            while (cancellationToken.IsCancellationRequested == false)
            {
                await moduleConnection.SendMessage("Test", $"This is a test message {i++}");
                
                await Task.Delay(100);
            }

            Console.WriteLine("Module end");
        }
    }
}
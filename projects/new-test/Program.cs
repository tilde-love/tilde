using Zero.Module;

namespace App
{
    // Main entry point for the the module    
    public class Program
    {
        public static void Main(string[] args)
        {
            ModuleRunner.Run(args[0], args[1], new Module());
        }
    }
}
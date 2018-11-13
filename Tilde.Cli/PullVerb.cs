using CommandLine;

namespace Tilde.Cli
{
    [Verb("pull", HelpText = "Pull a project from a tilde server.")]
    public class PullVerb : RemoteVerb
    {      
        [Value(1, MetaName = "path", HelpText = "Path to pull to.")]
        public string Path { get; set; }
        
//        [Option('d', "Path", Required = true, HelpText = "Path to pull to.")]
//        public string Path { get; set; }      
    }
}
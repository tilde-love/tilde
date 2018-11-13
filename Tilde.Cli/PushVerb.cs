using CommandLine;

namespace Tilde.Cli
{
    [Verb("push", HelpText = "Push a project to a tilde server.")]
    public class PushVerb : RemoteVerb
    {
        [Value(1, MetaName = "path", HelpText = "Path to push from.")]
        public string Path { get; set; }
        
//        [Option('d', "Path", Required = true, HelpText = "Path to push from.")]
//        public string Path { get; set; }   
    }
}
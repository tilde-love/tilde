using CommandLine;

namespace Tilde.Cli
{
    [Verb("serve", HelpText = "Start tilde server.")]
    public class ServeVerb
    {
        [Value(0, MetaName = "projects", Required = true, HelpText = "Path to project folder.")]
        public string ProjectFolder { get; set; }       
    }
}
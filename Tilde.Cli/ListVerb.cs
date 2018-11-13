using System;
using CommandLine;

namespace Tilde.Cli
{
    [Verb("ls", HelpText = "List elements.")]
    public class ListVerb
    {
        [Value(0, Default = ListableType.projects, HelpText = "Type", MetaName = "type")]
        public ListableType Type { get; set; } 
        
        [Value(1, MetaName = "project", HelpText = "Name of the project.")]
        public string Project { get; set; }

        [Option('s', "server", HelpText = "Tilde server uri.")]
        public Uri ServerUri { get; set; }
    }
}
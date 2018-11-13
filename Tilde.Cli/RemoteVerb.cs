using System;
using CommandLine;

namespace Tilde.Cli
{
    public abstract class RemoteVerb
    {
        [Value(0, MetaName = "project", Required = true, HelpText = "Name of the project.")]
        public string Project { get; set; }
        
        [Option('s', "server", HelpText = "Tilde server uri.")]
        public Uri ServerUri { get; set; }
    }
}
using System;
using CommandLine;

namespace Tilde.Cli
{
    [Verb("logs", HelpText = "Get the logs for a project.")]
    public class LogsVerb
    {
        [Value(0, MetaName = "project", HelpText = "Name of the project.")]
        public string Project { get; set; }
        
        [Option('s', "server", HelpText = "Tilde server uri.")]
        public Uri ServerUri { get; set; }
        
        [Option('f', "follow", HelpText = "Follow the log")]
        public bool Follow { get; set; } 
    }
}
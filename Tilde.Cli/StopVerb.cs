using CommandLine;

namespace Tilde.Cli
{
    [Verb("stop", HelpText = "Stop a project.")]
    public class StopVerb : RemoteVerb
    {
    }
}
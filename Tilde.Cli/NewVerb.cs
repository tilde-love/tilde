using CommandLine;

namespace Tilde.Cli
{
    [Verb("new", HelpText = "Create a new project.")]
    public class NewVerb : RemoteVerb
    {
    }
}
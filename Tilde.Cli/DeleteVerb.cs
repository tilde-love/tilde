using CommandLine;

namespace Tilde.Cli
{
    [Verb("delete", HelpText = "Delete a project.")]
    public class DeleteVerb : RemoteVerb
    {
    }
}
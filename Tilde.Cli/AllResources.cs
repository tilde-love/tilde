// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using Tilde.Cli.Resources;

namespace Tilde.Cli
{
    public static class AllResources
    {
        public static readonly List<CliResource> Resources = new List<CliResource>();

        public static readonly Dictionary<string, CliVerb> Verbs = new Dictionary<string, CliVerb>
        {
            {KnownVerbs.Add, new CliVerb(new[] {"add"}, "Add a resource.")},
            {KnownVerbs.Remove, new CliVerb(new[] {"remove", "rm"}, "Remove a resource.")},
            
            {KnownVerbs.Get, new CliVerb(new[] {"get"}, "Get a resource.")},
            {KnownVerbs.Set, new CliVerb(new[] {"set"}, "Set a resource.")},
            
            {KnownVerbs.List, new CliVerb(new[] {"list", "ls"}, "Lists a resource.")},
            {KnownVerbs.Delete, new CliVerb(new[] {"delete", "del"}, "Delete a resource.")},
            {KnownVerbs.New, new CliVerb(new[] {"new", "create", "n"}, "Create a new resource.")},

            {KnownVerbs.Pull, new CliVerb(new[] {"pull"}, "Pull a resource from a remote location.")},
            {KnownVerbs.Push, new CliVerb(new[] {"push"}, "Push a resource to a remote location.")},
            {KnownVerbs.Watch, new CliVerb(new[] {"watch"}, "Watch a local a resource and push any changes to a remote location. process or service.")},
            {KnownVerbs.Logs, new CliVerb(new[] {"logs"}, "Get resource logs from a remote location.")},

            {KnownVerbs.Start, new CliVerb(new[] {"start"}, "Start a resource process or service.")},
            {KnownVerbs.Stop, new CliVerb(new[] {"stop"}, "Stop a resource process or service.")}
        };

        public static void AddDefaultResources()
        {
            Resources.Add(new ProjectResource());
            Resources.Add(new FileResource());
            Resources.Add(new TemplateResource());
            Resources.Add(new TemplateIndexResource());
            Resources.Add(new ControlResource());
            Resources.Add(new WorkResource());
        }

        public static IEnumerable<Command> GetCommands()
        {
            List<string> verbNames = Resources.SelectMany(d => d.VerbCommands.Keys)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            ILookup<string, Command> verbCommandLookup = Resources.SelectMany(d => d.VerbCommands)
                .ToLookup(pair => pair.Key, pair => pair.Value);

            Dictionary<string, Command> macrosLookup = Resources.SelectMany(d => d.Commands)
                .ToDictionary(pair => pair.Name);

            List<string> allNames = verbNames.Union(macrosLookup.Keys)
                .ToList()
                .OrderBy(s => s)
                .ToList();

            foreach (string name in allNames)
            {
                if (verbNames.Contains(name))
                {
                    if (Verbs.TryGetValue(name, out CliVerb verb))
                    {
                        Command verbCommand = new Command(verb.Name, verb.Description);

                        foreach (string alias in verb.Aliases)
                        {
                            verbCommand.AddAlias(alias);
                        }

                        foreach (Command command in verbCommandLookup[name])
                        {
                            verbCommand.AddCommand(command);
                        }

                        yield return verbCommand;
                    }
                    else
                    {
                        Command verbCommand = new Command(name);

                        foreach (Command command in verbCommandLookup[name])
                        {
                            verbCommand.AddCommand(command);
                        }

                        yield return verbCommand;
                    }
                }
                else
                {
                    yield return macrosLookup[name];
                }
            }
        }
    }
}
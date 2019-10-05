// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CommandLine;
using CommandLine.Text;

namespace Tilde.Cli.Verbs
{
    [Verb("ls", HelpText = "List elements.")]
    public class ListVerb : Verb
    {
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(
                    "List projects",
                    new ListVerb
                    {
                        ItemTypes = ListableItemTypes.Projects,
                        ServerUri = new Uri(
                            "http://localhost:5678",
                            UriKind.RelativeOrAbsolute
                        )
                    }
                );

                yield return new Example(
                    "List files in a project",
                    new ListVerb
                    {
                        ItemTypes = ListableItemTypes.Files,
                        Project = "PROJECT",
                        ServerUri = new Uri(
                            "http://localhost:5678",
                            UriKind.RelativeOrAbsolute
                        )
                    }
                );

                yield return new Example(
                    "List controls in a project",
                    new ListVerb
                    {
                        ItemTypes = ListableItemTypes.Controls,
                        Project = "PROJECT",
                        ServerUri = new Uri(
                            "http://localhost:5678",
                            UriKind.RelativeOrAbsolute
                        )
                    }
                );
            }
        }

        [Value(0, Default = ListableItemTypes.Projects, HelpText = "ItemTypes", MetaName = "type")]
        public ListableItemTypes ItemTypes { get; set; }

        [Value(1, MetaName = "project", HelpText = "Name of the project.")]
        public string Project { get; set; }

        [Option('s', "server", HelpText = "Tilde server uri.")]
        public Uri ServerUri { get; set; }

        public static int List(ListVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            try
            {
                string[] items;

                switch (opts.ItemTypes)
                {
                    case ListableItemTypes.Projects:
                    {
                        Tuple<HttpStatusCode, Dictionary<Uri, ProjectResult>> responseTuple =
                            GetResponse<Dictionary<Uri, ProjectResult>>(
                                    "GET",
                                    new Uri(opts.ServerUri, new Uri("api/1.0/projects", UriKind.Relative))
                                )
                                .Result;

                        switch (responseTuple.Item1)
                        {
                            case HttpStatusCode.OK:
                                items = responseTuple.Item2.Keys.Select(u => u.ToString())
                                    .ToArray();
                                break;
                            default:
                                Console.WriteLine($"Unexpected status: {responseTuple.Item1}");
                                return -1;
                        }

                        break;
                    }

                    case ListableItemTypes.Files:
                    case ListableItemTypes.Controls:
                    {
                        Tuple<HttpStatusCode, ProjectResult> responseTuple =
                            GetResponse<ProjectResult>(
                                    "GET",
                                    new Uri(
                                        opts.ServerUri,
                                        new Uri(
                                            $"api/1.0/projects/{opts.Project}",
                                            UriKind.Relative
                                        )
                                    )
                                )
                                .Result;

                        switch (responseTuple.Item1)
                        {
                            case HttpStatusCode.OK when opts.ItemTypes == ListableItemTypes.Files:
                                items = responseTuple.Item2.Files.Select(u => u.Uri.ToString())
                                    .ToArray();
                                break;
                            case HttpStatusCode.OK when opts.ItemTypes == ListableItemTypes.Controls:
                                items = responseTuple.Item2.Controls.Sources.Values.Select(u => u.ToString())
                                    .ToArray();
                                break;

                            case HttpStatusCode.OK:
                                items = new string[0];
                                break;

                            case HttpStatusCode.NotFound:
                                Console.WriteLine($"Project {opts.Project} does not exist.");
                                return 0;

                            default:
                                Console.WriteLine($"Unexpected status: {responseTuple.Item1}");
                                return -1;
                        }

                        break;
                    }

                    case ListableItemTypes.Clients:
                        return 0;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Console.WriteLine(string.Join(Environment.NewLine, items));
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }
    }
}
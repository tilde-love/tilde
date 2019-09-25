// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;

namespace Tilde.Cli.Verbs
{
    [Verb("new", HelpText = "Create a new project.")]
    public class NewVerb : RemoteVerb
    {
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(
                    "Create a new project",
                    new LogsVerb
                    {
                        Project = "PROJECT",
                        ServerUri = new Uri(
                            "http://localhost:5678",
                            UriKind.RelativeOrAbsolute
                        )
                    }
                );
            }
        }

        public static int New(NewVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            Logo.PrintLogo();

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative));

                HttpStatusCode statusCode = MakeRequest("POST", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.Created:
                        Console.WriteLine($"Created project {opts.Project}");
                        ListVerb.List(
                            new ListVerb
                            {
                                ServerUri = opts.ServerUri,
                                ItemTypes = ListableItemTypes.Files,
                                Project = opts.Project
                            }
                        );
                        return 0;

                    case HttpStatusCode.Conflict:
                        Console.WriteLine($"Project {opts.Project} already exists.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }
    }
}
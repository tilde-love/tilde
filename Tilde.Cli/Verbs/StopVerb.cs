// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;

namespace Tilde.Cli.Verbs
{
    [Verb("stop", HelpText = "Stop a project.")]
    public class StopVerb : RemoteVerb
    {
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example(
                    "Stop a project",
                    new StartVerb
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

        public static int Stop(StopVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri("api/1.0/script/runtime/stop", UriKind.Relative));

                HttpStatusCode statusCode = MakeRequest("GET", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Stopped project {opts.Project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {opts.Project} not found.");
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
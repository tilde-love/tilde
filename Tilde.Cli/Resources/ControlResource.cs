// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;

namespace Tilde.Cli.Resources
{
    public class ControlResource : CliResource
    {
        public ControlResource()
        {
            Name = "control";
            Aliases.AddRange(new[] {"controls", "c"});
            Description = "Tilde project control.";

            VerbCommands[KnownVerbs.List] = CreateNounCommand(
                "List all controls in a tilde projects on a server.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri serverUri, Uri project) => List(serverUri, project)
                )
            );
        }

        private static int List(Uri serverUri, Uri project)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri(
                    $"api/1.0/projects/{project}",
                    UriKind.Relative
                )
            );

            try
            {
                string[] items;

                (HttpStatusCode statusCode, ProjectResult result) responseTuple =
                    RestApi.GetResponse<ProjectResult>("GET", requestUri)
                        .Result;

                switch (responseTuple.statusCode)
                {
                    case HttpStatusCode.OK:
                        items = responseTuple.Item2.Controls.Sources.Values.Select(u => u.ToString())
                            .ToArray();
                        break;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {project} does not exist.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {responseTuple.statusCode}");
                        return -1;
                }

                Console.WriteLine(string.Join(Environment.NewLine, items));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }
    }
}
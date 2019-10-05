// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;
using CommandLine.Text;

namespace Tilde.Cli.Verbs
{
    [Verb("delete", HelpText = "Delete a project.")]
    public class DeleteVerb : RemoteVerb
    {
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples
        {
            get { yield return new Example("Delete a project", new DeleteVerb {Project = "PROJECT", ServerUri = new Uri("http://localhost:5678", UriKind.RelativeOrAbsolute)}); }
        }

        public static int Delete(DeleteVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative));

                HttpStatusCode statusCode = MakeRequest("DELETE", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Deleted project {opts.Project}");
                        // ListVerb.List(new ListVerb {ServerUri = opts.ServerUri, ItemTypes = ListableItemTypes.Projects});
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {opts.Project} does not exist.");
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
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;

namespace Tilde.Cli.Resources
{
    public class FileResource : CliResource
    {
        public static Option PathOption()
        {
            return new Option(
                new[]
                {
                    "--path",
                    "-p"
                },
                "A file path.",
                new Argument<Uri>()
                {
                    Name = "path",
                }
            );
        }
        
        public static Option OutputPathOption()
        {
            return new Option(
                new[]
                {
                    "--output",
                    "-o"
                },
                "Output file path.",
                new Argument<Uri>()
                {
                    Name = "path",
                }
            );
        }
        
        public FileResource()
        {
            Name = "file";
            Aliases.AddRange(new[] {"files", "f"});
            Description = "Tilde project file.";

            VerbCommands[KnownVerbs.Delete] = CreateNounCommand(
                "Delete a tilde project file.",
                new[] {
                    CommonArguments.ServerUriOption(),  
                    //PathOption(),
                },
                CommonArguments.ProjectPathArgument(),
                CommandHandler.Create(
                    (Uri projectPath, Uri serverUri) => Delete(projectPath, serverUri)
                )
            );

            VerbCommands[KnownVerbs.List] = CreateNounCommand(
                "List all files in a tilde projects on a server.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri serverUri, Uri project) => List(project, serverUri)
                )
            );

            VerbCommands[KnownVerbs.Pull] = CreateNounCommand(
                "Pull a project file from a tilde server.",
                new[] {
                    CommonArguments.ServerUriOption(),
                },
                new Argument<string>
                {
                    Arity = new ArgumentArity(1, 2),
                    Name = "paths",
                    Description = "Project file path. Optionally file system path."
                },
                CommandHandler.Create(
                    (List<string> paths, Uri serverUri) => Pull(paths, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.Push] = CreateNounCommand(
                "Push a project file to a tilde server.",
                new[] {
                    CommonArguments.ServerUriOption(),
                },
                new Argument<string>
                {
                    Arity = new ArgumentArity(1, 2),
                    Name = "paths",
                    Description = "Project file path. Optionally file system path."
                },
                CommandHandler.Create(
                    (List<string> paths, Uri serverUri) => Push(paths, serverUri)
                )
            );
        }

        private int Pull(List<string> paths, Uri serverUri)
        {
            try
            {
                string projectFile = paths.FirstOrDefault();
                string localFile = paths.Count == 1 ? projectFile : paths[1];

                RestApi.DownloadFile(serverUri, projectFile, localFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }

                return -1;
            }

            return 0;
        }

        private int Push(List<string> paths, Uri serverUri)
        {
            try
            {
                string projectFile = paths.FirstOrDefault();
                string localFile = paths.Count == 1 ? projectFile : paths[1];

                RestApi.UploadFile(serverUri, projectFile, localFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }

                return -1;
            }

            return 0;
        }

        private static int Delete(Uri projectPath, Uri serverUri)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{projectPath}", UriKind.Relative));
            
            try
            {

                HttpStatusCode statusCode = RestApi.MakeRequest("DELETE", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Deleted file {projectPath}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"File {projectPath} does not exist.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private static int List(Uri project, Uri serverUri)
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
                        items = responseTuple.result.Files.Select(u => u.Uri.ToString())
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
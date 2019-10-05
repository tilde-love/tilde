// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tilde.Core.Controls;
using Tilde.Core.Projects;

namespace Tilde.Cli.Resources
{
    public class ProjectResource : CliResource
    {
        public ProjectResource()
        {
            Name = "project";
            Aliases.AddRange(new[] {"projects", "p"});
            Description = "Tilde project.";

            VerbCommands[KnownVerbs.Delete] = CreateNounCommand(
                "Delete a tilde project.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri) => Delete(serverUri, project)
                )
            );

            VerbCommands[KnownVerbs.List] = CreateNounCommand(
                "List all tilde projects on a server.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                null,
                CommandHandler.Create(
                    (Uri serverUri) => List(serverUri)
                )
            );

            VerbCommands[KnownVerbs.New] = CreateNounCommand(
                "Create a new tilde project.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri) => New(serverUri, project)
                )
            );

            VerbCommands[KnownVerbs.Pull] = CreateNounCommand(
                "Pull a project from a tilde server.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                    new Option(
                        new[]
                        {
                            "--path",
                            "-p"
                        },
                        "Path to pull to.",
                        new Argument<string>("./")
                        {
                            Name = "path"
                        }
                    )
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri, string path) => Pull(serverUri, project, path)
                )
            );

            VerbCommands[KnownVerbs.Push] = CreateNounCommand(
                "Push a project to a tilde server.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                    new Option(
                        new[]
                        {
                            "--path",
                            "-p"
                        },
                        "Path to push from.",
                        new Argument<string>("./")
                        {
                            Name = "path"
                        }
                    )
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri, string path) => Push(serverUri, project, path)
                )
            );

            VerbCommands[KnownVerbs.Start] = CreateNounCommand(
                "Start a project.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri) => Start(serverUri, project)
                )
            );

            VerbCommands[KnownVerbs.Stop] = CreateNounCommand(
                "Stop a project.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri) => Stop(serverUri, project)
                )
            );

            VerbCommands[KnownVerbs.Watch] = CreateNounCommand(
                "Watch a local folder and upload any changes.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                    new Option(
                        new[]
                        {
                            "--path",
                            "-p"
                        },
                        "Path of project in the file system to watch.",
                        new Argument<string>("./")
                        {
                            Name = "path"
                        }
                    )
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri, string path) => Watch(project, serverUri, path)
                )
            );

            VerbCommands[KnownVerbs.Logs] = CreateNounCommand(
                "Get the logs for a project.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                    new Option(
                        new[]
                        {
                            "--follow",
                            "-f"
                        },
                        "Follow the log.",
                        new Argument<bool>(false)
                        {
                            Name = "follow"
                        }
                    )
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri project, Uri serverUri, bool follow) => Logs(project, serverUri, follow)
                )
            );
        }

        private static int Delete(Uri serverUri, Uri project)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{project}", UriKind.Relative));

            try
            {
                HttpStatusCode statusCode = RestApi.MakeRequest("DELETE", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Deleted project {project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {project} does not exist.");
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

        private static int List(Uri serverUri)
        {
            Uri requestUri = new Uri(serverUri, new Uri("api/1.0/projects", UriKind.Relative));

            try
            {
                string[] items;

                (HttpStatusCode statusCode, Dictionary<Uri, ProjectResult> result) responseTuple =
                    RestApi.GetResponse<Dictionary<Uri, ProjectResult>>("GET", requestUri)
                        .Result;

                switch (responseTuple.statusCode)
                {
                    case HttpStatusCode.OK:
                        items = responseTuple.result.Keys.Select(u => u.ToString())
                            .ToArray();
                        break;
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

        private int Logs(Uri project, Uri serverUri, bool follow)
        {
            if (RestApi.GetRawFile(serverUri, project.ToString(), "build") == false)
            {
                return -1;
            }

            if (RestApi.GetRawFile(serverUri, project.ToString(), "log") == false)
            {
                return -1;
            }


            return 0;
        }

        private static int New(Uri serverUri, Uri project)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{project}", UriKind.Relative));

                HttpStatusCode statusCode = RestApi.MakeRequest("POST", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.Created:
                        Console.WriteLine($"Created project {project}");
                        return 0;

                    case HttpStatusCode.Conflict:
                        Console.WriteLine($"Project {project} already exists.");
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

        private static int Pull(Uri serverUri, Uri project, string path)
        {
            (HttpStatusCode statusCode, ProjectResult result) result;

            Uri requestUri = new Uri(
                serverUri,
                new Uri($"api/1.0/projects/{project}", UriKind.Relative)
            );

            try
            {
                result = RestApi.GetResponse<ProjectResult>("GET", requestUri)
                    .Result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);

                return -1;
            }

            using (Project projectObj = new Project(new DirectoryInfo(path)))
            {
                try
                {
                    switch (result.Item1)
                    {
                        case HttpStatusCode.OK:
                            List<Uri> filesWithoutPeers = result.Item2.Files.Select(p => p.Uri)
                                .ToList();

                            foreach (ProjectFile projectFile in projectObj.Files)
                            {
                                if (filesWithoutPeers.Contains(projectFile.Uri) == false)
                                {
                                    Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash}) [NO PEER]");

                                    projectObj.DeleteFile(projectFile.Uri);
                                }
                                else
                                {
                                    filesWithoutPeers.Remove(projectFile.Uri);
                                }
                            }

                            foreach (ProjectFile projectFile in result.Item2.Files)
                            {
                                if (projectObj.ProjectFiles.TryGetValue(projectFile.Uri, out ProjectFile localFile)
                                    && projectFile.Hash.Equals(localFile.Hash))
                                {
                                    Console.WriteLine($"{projectFile.Uri} ({localFile.Hash})");

                                    continue;
                                }

                                string filePath = Path.Combine(
                                    path,
                                    projectFile.Uri.ToString()
                                        .TrimStart('/')
                                );

                                string directory = new FileInfo(filePath).DirectoryName;

                                if (Directory.Exists(directory) == false)
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                RestApi.DownloadFile(serverUri, $"{project}/{projectFile.Uri}", filePath);

                                Console.WriteLine($"{projectFile.Uri} ({localFile.Hash ?? "NO PEER"}) [{projectFile.Hash}]");
                            }

                            return 0;

                        default:
                            Console.WriteLine($"Unexpected status: {result}");
                            return -1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    return -1;
                }
            }
        }

        private static int Push(Uri serverUri, Uri project, string path)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri($"api/1.0/projects/{project}", UriKind.Relative)
            );

            (HttpStatusCode statusCode, ProjectResult result) result;

            try
            {
                result = RestApi.GetResponse<ProjectResult>("GET", requestUri)
                    .Result;

                switch (result.Item1)
                {
                    case HttpStatusCode.OK: break;
                    case HttpStatusCode.NotFound:
                        result = (
                            result.Item1,
                            new ProjectResult
                            {
                                Uri = new Uri($"{project}", UriKind.Relative),
                                Files = new List<ProjectFile>(),
                                Controls = new ControlGroup()
                            }
                        );
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);

                return -1;
            }

            string basePath = path;
            string basePathFull = new DirectoryInfo(path).FullName;

            using (Project projectObj = new Project(new DirectoryInfo(path)))
            {
                List<Uri> filesWithoutPeers = result.Item2.Files.Select(p => p.Uri)
                    .ToList();

                Dictionary<Uri, ProjectFile> remoteProjectFiles = result.Item2.Files.ToDictionary(x => x.Uri);

                foreach (ProjectFile projectFile in projectObj.Files)
                {
                    if (filesWithoutPeers.Contains(projectFile.Uri) != true)
                    {
                        continue;
                    }

                    filesWithoutPeers.Remove(projectFile.Uri);
                }

                foreach (ProjectFile projectFile in projectObj.Files)
                {
                    if (remoteProjectFiles.TryGetValue(projectFile.Uri, out ProjectFile remoteProjectFile)
                        && projectFile.Hash.Equals(remoteProjectFile.Hash))
                    {
                        Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash})");

                        continue;
                    }

                    if (filesWithoutPeers.Contains(projectFile.Uri))
                    {
                        RestApi.DeleteFile(serverUri, project.ToString(), projectFile.Uri.ToString());

                        Console.WriteLine($"{projectFile.Uri} (NO PEER) [{remoteProjectFile.Hash}]");

                        continue;
                    }

                    RestApi.UploadFile(serverUri, project.ToString(), projectFile.Uri.ToString(), projectObj.GetFilePath(projectFile.Uri));

                    Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash}) [{remoteProjectFile.Hash ?? "NO PEER"}]");
                }
            }

            return 0;
        }

        private static int Start(Uri serverUri, Uri project)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri(
                    $"api/1.0/script/runtime/run/{project}",
                    UriKind.Relative
                )
            );

            try
            {
                HttpStatusCode statusCode = RestApi.MakeRequest("GET", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Started project {project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {project} not found.");
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

        private static int Stop(Uri serverUri, Uri project)
        {
            Uri requestUri = new Uri(serverUri, new Uri("api/1.0/script/runtime/stop", UriKind.Relative));

            try
            {
                HttpStatusCode statusCode = RestApi.MakeRequest("GET", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Stopped project {project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {project} not found.");
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

        private static int Watch(Uri project, Uri serverUri, string path)
        {
            bool cancel = false;

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs args)
            {
                cancel = true;
                args.Cancel = true;
            };

            Push(project, serverUri, path);

            using (Project projectObj = new Project(new DirectoryInfo(path)))
            {
                projectObj.FileChanged += delegate(
                    Uri uri,
                    Uri file,
                    string hash)
                {
                    if (hash == null)
                    {
                        RestApi.DeleteFile(
                            serverUri,
                            project.ToString(),
                            file.ToString()
                        );

                        Console.WriteLine($"{file} (DELETED)");
                    }
                    else
                    {
                        RestApi.UploadFile(
                            serverUri,
                            project.ToString(),
                            file.ToString(),
                            projectObj.GetFilePath(file)
                        );

                        Console.WriteLine($"{file} ({hash})");
                    }
                };

                while (cancel == false)
                {
                    Task.Delay(100)
                        .Wait();
                }
            }

            return 0;
        }
    }
}
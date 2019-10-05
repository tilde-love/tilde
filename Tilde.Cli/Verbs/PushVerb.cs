// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;
using Tilde.Core.Controls;
using Tilde.Core.Projects;

namespace Tilde.Cli.Verbs
{
    [Verb("push", HelpText = "Push a project to a tilde server.")]
    public class PushVerb : RemoteVerb
    {
        [Value(1, MetaName = "path", HelpText = "Path to push from.")]
        public string Path { get; set; }

//        [Option('d', "Path", Required = true, HelpText = "Path to push from."
//        public string Path { get; set; }           

        public static int Push(PushVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            // Logo.PrintLogo(0, 3);
            
            // Console.WriteLine($"Pushing project {opts.Project}");
            
            Tuple<HttpStatusCode, ProjectResult> result; 
            
            try
            {
                Uri requestUri = new Uri(
                    opts.ServerUri,
                    new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative)
                );

                result = GetResponse<ProjectResult>("GET", requestUri).Result;

                switch (result.Item1)
                {
                    case HttpStatusCode.OK: break;
                    case HttpStatusCode.NotFound:
                        result = new Tuple<HttpStatusCode, ProjectResult>(
                            result.Item1,
                            new ProjectResult()
                            {
                                Uri = new Uri($"{opts.Project}", UriKind.Relative),
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
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
            
            string basePath = opts.Path;
            string basePathFull = new DirectoryInfo(opts.Path).FullName;

            using (Core.Projects.Project project = new Project(new DirectoryInfo(opts.Path)))
            {
                List<Uri> filesWithoutPeers = result.Item2.Files.Select(p => p.Uri).ToList();
                
                Dictionary<Uri, ProjectFile> remoteProjectFiles = result.Item2.Files.ToDictionary(x => x.Uri); 
                
                foreach (ProjectFile projectFile in project.Files)
                {
                    if (filesWithoutPeers.Contains(projectFile.Uri) != true)
                    {
                        continue;
                    }

                    filesWithoutPeers.Remove(projectFile.Uri);
                }

                foreach (ProjectFile projectFile in project.Files)
                {
                    if (remoteProjectFiles.TryGetValue(projectFile.Uri, out ProjectFile remoteProjectFile) 
                        && projectFile.Hash.Equals(remoteProjectFile.Hash))
                    {
                        Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash})");
                        
                        continue; 
                    }

                    if (filesWithoutPeers.Contains(projectFile.Uri))
                    {
                        DeleteFile(opts.ServerUri, opts.Project, projectFile.Uri.ToString());

                        Console.WriteLine($"{projectFile.Uri} (NO PEER) [{remoteProjectFile.Hash}]");
                        
                        continue; 
                    }                    
                        
                    UploadFile(opts.ServerUri, opts.Project, projectFile.Uri.ToString(), project.GetFilePath(projectFile.Uri));

                    Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash}) [{remoteProjectFile.Hash ?? "NO PEER"}]");
                }
            }

            return 0;
        }
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Cli.Verbs
{
    [Verb("pull", HelpText = "Pull a project from a tilde server.")]
    public class PullVerb : RemoteVerb
    {
        [Value(1, MetaName = "path", HelpText = "Path to pull to.")]
        public string Path { get; set; }

//        [Option('d', "Path", Required = true, HelpText = "Path to pull to.")]
//        public string Path { get; set; }              

        public static int Pull(PullVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            // Logo.PrintLogo(0, 3);

            Tuple<HttpStatusCode, ProjectResult> result; 
            
            try
            {
                Uri requestUri = new Uri(
                    opts.ServerUri,
                    new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative)
                );

                result = GetResponse<ProjectResult>("GET", requestUri).Result;                
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }

            using (Core.Projects.Project project = new Project(new DirectoryInfo(opts.Path)))
            {
//                Console.WriteLine(project.ProjectFolder);
//
//                foreach (ProjectFile projectFile in project.Files)
//                {
//                    Console.WriteLine($"{projectFile.Uri} [{projectFile.Hash}]");
//                }

                try
                {
                    switch (result.Item1)
                    {
                        case HttpStatusCode.OK:
                            List<Uri> filesWithoutPeers = result.Item2.Files.Select(p => p.Uri).ToList(); 
                            
                            foreach (ProjectFile projectFile in project.Files)
                            {
                                if (filesWithoutPeers.Contains(projectFile.Uri) == false)
                                {
                                    Console.WriteLine($"{projectFile.Uri} ({projectFile.Hash}) [NO PEER]");
                                    
                                    project.DeleteFile(projectFile.Uri);
                                }
                                else
                                {
                                    filesWithoutPeers.Remove(projectFile.Uri);
                                }
                            }

                            foreach (ProjectFile projectFile in result.Item2.Files)
                            {                                
                                if (project.ProjectFiles.TryGetValue(projectFile.Uri, out ProjectFile localFile) 
                                    && projectFile.Hash.Equals(localFile.Hash))
                                {
                                    Console.WriteLine($"{projectFile.Uri} ({localFile.Hash})");
                                    
                                    continue; 
                                }

                                string filePath = System.IO.Path.Combine(
                                    opts.Path,
                                    projectFile.Uri.ToString()
                                        .TrimStart('/')
                                );
                                                                                                
                                string directory = new FileInfo(filePath).DirectoryName;

                                if (Directory.Exists(directory) == false)
                                {
                                    Directory.CreateDirectory(directory);
                                }

                                File.WriteAllBytes(
                                    filePath,
                                    DownloadFile(opts.ServerUri, opts.Project, projectFile.Uri.ToString())
                                );

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
    }
}
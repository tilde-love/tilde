// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommandLine;

namespace Tilde.Cli.Verbs
{
    [Verb("watch", HelpText = "Watch a local folder and upload any changes.")]
    public class WatchVerb : RemoteVerb
    {
        [Value(1, MetaName = "path", HelpText = "Path of project to watch.")]
        public string Path { get; set; }
        
        public static int Watch(WatchVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5678/", UriKind.RelativeOrAbsolute);
            }

            // Logo.PrintLogo(0, 3);

            bool cancel = false;

            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs args)
            {
                cancel = true;
                args.Cancel = true;
            };

            PushVerb push = new PushVerb
            {
                Path = opts.Path, Project = opts.Project,
                ServerUri = opts.ServerUri
            };

            PushVerb.Push(push);
            
            using (Core.Projects.Project project = new Core.Projects.Project(new DirectoryInfo(opts.Path)))
            {
                project.FileChanged += delegate(
                    Uri uri,
                    Uri file,
                    string hash)
                {
                    if (hash == null)
                    {
                        DeleteFile(
                            opts.ServerUri,
                            opts.Project,
                            file.ToString()
                        );
                        
                        Console.WriteLine($"{file} (DELETED)");
                    }
                    else
                    {
                        UploadFile(
                            opts.ServerUri,
                            opts.Project,
                            file.ToString(),
                            project.GetFilePath(file)
                        );
                        
                        Console.WriteLine($"{file} ({hash})");
                    }
                };

                while (cancel == false)
                {
                    Task.Delay(100).Wait(); 
                }
            }

            return 0;
        }
    }
}
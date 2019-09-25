// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using Tilde.Core.Templates;

namespace Tilde.Cli.Resources
{
    public class TemplateResource : CliResource
    {
        public TemplateResource()
        {
            Name = "template";
            Aliases.AddRange(new[] {"templates", "t"});
            Description = "Tilde project template.";

            // list, get a list of all templates
            // push, upload a template 
            // pull, download a template
            // delete, delete a template
            
            // add remove, pull, list, 
//            VerbCommands[KnownVerbs.Delete] = CreateNounCommand(
//                "Delete a tilde project file.",
//                new[]
//                {
//                    CommonArguments.ServerUriOption(),
//                    //PathOption(),
//                },
//                CommonArguments.ProjectPathArgument(),
//                CommandHandler.Create(
//                    (Uri projectPath, Uri serverUri) => Delete(projectPath, serverUri)
//                )
//            );

//            Commands.Add(
//                new Command(
//                    "pack",
//                    @"Pack a template index.",
//                    null,
//                    new Argument<string>
//                    {
//                        Arity = new ArgumentArity(1, 2),
//                        Name = "paths",
//                        Description = "Source path. Optional output path."
//                    },
//                    handler: CommandHandler.Create<List<string>>(
//                        (paths) => Pack(paths)
//                    )
//                )
//            );
        }
//
//        private int Pack(List<string> paths)
//        {
//            try
//            {
//                string source = paths.FirstOrDefault();
//                string destination = paths.Count == 1 ? source : paths[1];
//                
//                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(source);
//                DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(destination);
//
//                TemplateIndex.Pack(sourceDirectoryInfo, destinationDirectoryInfo);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message);
//
//                if (e.InnerException != null)
//                {
//                    Console.WriteLine(e.InnerException.Message);
//                }
//
//                return -1;
//            }
//
//            return 0;
//                        
//        }
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Tilde.Cli.Verbs
{
    [Verb("logs", HelpText = "Get the logs for a project.")]
    public class LogsVerb : Verb
    {
        [Value(0, MetaName = "project", HelpText = "Name of the project.")]
        public string Project { get; set; }
        
        [Option('s', "server", HelpText = "Tilde server uri.")]
        public Uri ServerUri { get; set; }
        
        [Option('f', "follow", HelpText = "Follow the log")]
        public bool Follow { get; set; } 
                             
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples 
        {
            get 
            {
                yield return new Example("Get project log", new LogsVerb { Project = "PROJECT", ServerUri = new Uri("http://localhost:5000", UriKind.RelativeOrAbsolute) });
            }
        }
        
        public static int Logs(LogsVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5000/", UriKind.RelativeOrAbsolute); 
            }
            
            try
            {
                if (GetRawFile(opts, "build") == false)
                {
                    return -1;
                }

                if (GetRawFile(opts, "log") == false)
                {
                    return -1;
                }
            }            
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }
    }
}
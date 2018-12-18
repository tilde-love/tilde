// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using CommandLine;

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
                opts.ServerUri = new Uri("http://localhost:5000/", UriKind.RelativeOrAbsolute); 
            }

            // Logo.PrintLogo(0, 3);

            return 0;
        }
    }
}
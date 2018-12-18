// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using CommandLine;

namespace Tilde.Cli.Verbs
{
    [Verb("push", HelpText = "Push a project to a tilde server.")]
    public class PushVerb : RemoteVerb
    {
        [Value(1, MetaName = "path", HelpText = "Path to push from.")]
        public string Path { get; set; }
        
//        [Option('d', "Path", Required = true, HelpText = "Path to push from.")]
//        public string Path { get; set; }           
        
        public static int Push(PushVerb opts)
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
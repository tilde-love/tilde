// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using CommandLine;

namespace Tilde.Cli.Verbs
{
    [Verb("watch", HelpText = "Watch a local folder and upload any changes.")]
    public class WatchVerb : RemoteVerb
    {   
        public static int Watch(WatchVerb opts)
        {
            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5000/", UriKind.RelativeOrAbsolute); 
            }

            Logo.PrintLogo(0, 3);

            return 0;
        }
    }
}
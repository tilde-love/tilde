// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using Tilde.Cli.Verbs;

namespace Tilde
{
    [Verb("serve", HelpText = "Start tilde server.")]
    public class ServeVerb
    {
        [Value(0, MetaName = "projects", HelpText = "Path to project folder. If none is supplied then the current directory is used as the project root folder.")]
        public string ProjectFolder { get; set; }       
        
        [Value(1, MetaName = "uri", HelpText = "The uri the server should use to listen on.")]
        public Uri ServerUri { get; set; }      
        
        [Value(1, MetaName = "wwwroot", HelpText = "The path that contains the html resources for the web portal. If none is supplied then a 'wwwroot' sub-directory the tilde executable path is used.")]
        public string WwwRoot { get; set; }      
        
        [Usage(ApplicationAlias = "tilde")]
        public static IEnumerable<Example> Examples 
        {
            get 
            {
                yield return new Example("Default", new ServeVerb { ProjectFolder = "./", ServerUri = new Uri("http://localhost:5000", UriKind.RelativeOrAbsolute) });
                yield return new Example("Local", new ServeVerb { ProjectFolder = "./projects", ServerUri = new Uri("http://localhost:5000", UriKind.RelativeOrAbsolute), WwwRoot = "./wwwroot" });
                yield return new Example("Deployed", new ServeVerb { ProjectFolder = "./projects", ServerUri = new Uri("http://0.0.0.0:80", UriKind.RelativeOrAbsolute), WwwRoot = "./wwwroot" });
            }
        }

        //  e.g http://0.0.0.0:80 (port 80 on any network interface), http://localhost:5000 (default)
    }
}
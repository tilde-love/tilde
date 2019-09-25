// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using Tilde.Cli;
using Tilde.Cli.Resources;

namespace Tilde
{
    public class Program
    {
        public static int Main(string[] args)
        {
            AllResources.AddDefaultResources();
            
            AllResources.Resources.Add(new ServerResource());
            
            RootCommand root = new RootCommand("tilde")
            {
                Description = "Tiny Integrated Live(ish) Development Environment"
            };

            foreach (var command in AllResources.GetCommands())
            {
                root.Add(command);
            }

            var parser = new CommandLineBuilder(root)
                .UseSuggestDirective()
                .UseTypoCorrections()
                .UseHelp()
                //.UseHelpBuilderFactory()
                .Build();

            var parseResult = parser.Parse(args);
            
            // Console.WriteLine(parseResult.Diagram());

            return parser.InvokeAsync(parseResult).Result;
        }
    }
}
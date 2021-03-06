﻿// Copyright (c) Tilde Love Project. All rights reserved.
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
                Description = "Tilde Love - Containerisation for artists."
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

            string[] a = args.Length > 0 ? args : new [] { "--help" };

            var parseResult = parser.Parse(a);

            try
            {
                return parser.InvokeAsync(parseResult).Result;
            }
            catch (Exception ex) 
            {
                if (parseResult.Errors.Count > 0)
                {
                    foreach (var error in parseResult.Errors)
                    {
                        Console.Error.WriteLine(error.Message);
                    }

                    return -1; 
                }

                throw; 
            }
        }
    }
}
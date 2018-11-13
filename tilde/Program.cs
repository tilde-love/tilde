// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CommandLine;
using CommandLine.Text;
using CSharpx;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Tilde.Cli;

namespace Tilde
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                // .SuppressStatusMessages(true) //disable the status messages
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True") // add this line
                .UseStartup<Startup>()
                .Build();
        }                

        public static int Main(string[] args)
        {
            ParserSettings settings = new ParserSettings();

            //settings.
            
            Parser parser = new Parser();
            
            List<Type> verbs = new List<Type>();
            
            verbs.Add(typeof(ServeVerb));

            verbs.AddRange(Commands.Verbs);

            ParserResult<object> parserResult = parser.ParseArguments(
                args,
                verbs.ToArray()
            );
//            
//            ParserResult<object> parserResult = parser.ParseArguments<                   
//                ServeVerb,
//                ListVerb, 
//                NewVerb,
//                DeleteVerb,
//                StartVerb,
//                StopVerb, 
//                PullVerb,
//                PushVerb, 
//                WatchVerb,
//                LogsVerb
//                >(args);
            //parserResult.WithParsed<MyOptions>(options => DoSomething(options));

            // Console.WriteLine(HelpText.AutoBuild(parserResult, null, null));
            
            parserResult.WithParsed((ServeVerb opts) => Serve(opts));
            
            Commands.Parse(parserResult);
            
            Commands.ParseErrors(parserResult);
            
            return 0; 

//            return Parser.Default
//                .ParseArguments<
//                    ServeVerb, 
//                    NewVerb, 
//                    StartVerb,
//                    StopVerb, 
//                    PullVerb,
//                    PushVerb, 
//                    WatchVerb,
//                    LogsVerb
//                >(args)
//                .MapResult(
//                    (ServeVerb opts) => Serve(opts),
//                    (NewVerb opts) => Commands.New(opts),       
//                    (StartVerb opts) => Commands.Start(opts),
//                    (StopVerb opts) => Commands.Stop(opts),
//                    (PullVerb opts) => Commands.Pull(opts),                   
//                    (PushVerb opts) => Commands.Push(opts),
//                    (WatchVerb opts) => Commands.Watch(opts),
//                    (LogsVerb opts) => Commands.Logs(opts),
//                    errs => 1                    
//                );
        }        

        public static int Serve(ServeVerb opts)
        {                        
            Logo.PrintLogo();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                BuildWebHost(new string[0]).Run();

                return 0; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
                return -1; 
            }                       
        }        

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
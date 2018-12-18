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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tilde.Cli;

namespace Tilde
{
    public class Program
    {
        public static IWebHostBuilder BuildWebHost(ServeVerb opts, string[] args)
        {
            string applicationPath = new FileInfo(typeof(Program).Assembly.Location).DirectoryName; 
            
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                // .SuppressStatusMessages(true) //disable the status messages
                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.ServerUri)}", opts.ServerUri.ToString())
                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.ProjectFolder)}", opts.ProjectFolder)
                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.WwwRoot)}", opts.WwwRoot)
                .UseUrls(opts.ServerUri.ToString())
                .UseWebRoot(opts.WwwRoot)
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True") // add this line
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config
                        .AddJsonFile(Path.Combine(applicationPath, "appsettings.json"), true, true)
                        .AddJsonFile(Path.Combine(applicationPath, $"appsettings.{env.EnvironmentName}.json"), true, true);
                    config.AddEnvironmentVariables();
                })
//                .ConfigureLogging((hostingContext, logging) =>
//                {
//                    logging.ClearProviders(); 
////                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
////                    logging.AddConsole();
////                    logging.AddDebug();
////                    logging.AddEventSourceLogger();
//                })
                .UseStartup<Startup>();
            
            return builder;
        }                

        public static int Main(string[] args)
        {
            Parser parser = new Parser(
                parserSettings => { parserSettings.CaseInsensitiveEnumValues = true; } 
            );          
            
            List<Type> verbs = new List<Type>();
            
            verbs.Add(typeof(ServeVerb));

            verbs.AddRange(Commands.Verbs);

            ParserResult<object> parserResult = parser.ParseArguments(
                args,
                verbs.ToArray()
            );
            
            parserResult.WithParsed((ServeVerb opts) => Serve(opts));
            
            Commands.Parse(parserResult);
            
            Commands.ParseErrors(parserResult);
            
            return 0; 
        }        

        public static int Serve(ServeVerb opts)
        {
            if (string.IsNullOrWhiteSpace(opts.ProjectFolder))
            {
                opts.ProjectFolder = "./"; 
            }

            if (opts.ServerUri == null)
            {
                opts.ServerUri = new Uri("http://localhost:5000", UriKind.RelativeOrAbsolute); 
            }

            string applicationPath = new FileInfo(typeof(Program).Assembly.Location).DirectoryName; 
            
            if (string.IsNullOrWhiteSpace(opts.WwwRoot))
            {
                opts.WwwRoot = string.IsNullOrWhiteSpace(opts.WwwRoot) ? Path.Combine(applicationPath, "wwwroot") : opts.WwwRoot; 
            }

            Logo.PrintLogo();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                BuildWebHost(opts, new string[0])
                    .Build()
                    .Run();

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
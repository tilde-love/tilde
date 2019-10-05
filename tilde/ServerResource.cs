// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Tilde.Cli;
using Tilde.Cli.Resources;

namespace Tilde
{
    class ServerResource : CliResource
    {
        public ServerResource()
        {
            Name = "server";
            Description = "Tilde server";

            VerbCommands[KnownVerbs.Start] = CreateNounCommand(
                "Start a tilde server instance.",
                CreateServeOptions(),
                CreateProjectFolderArgument(),
                CommandHandler.Create<string, Uri, string, string, bool>(
                    (projects, serverUri, templates, wwwRoot, noLogo) => Serve(projects, serverUri, templates, wwwRoot, noLogo)
                )
            );

            Commands.Add(
                new Command(
                    "serve",
                    @"Start a tilde server instance. Equivalent of running ""tilde start server""",
                    CreateServeOptions(),
                    CreateProjectFolderArgument(),
                    handler: CommandHandler.Create<string, Uri, string, string, bool>(
                        (projects, serverUri, templates, wwwRoot, noLogo) => Serve(projects, serverUri,templates, wwwRoot, noLogo)
                    )
                )
            );
        }

        private string GetApplicationDataFolder(string subFolder)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify), "tilde", subFolder);
        }

        private Argument CreateProjectFolderArgument() =>
            new Argument<string>(GetApplicationDataFolder("projects"))
            {
                Arity = ArgumentArity.ExactlyOne,
                Name = "projects",
                Description =
                    "Path to project folder. " +
                    "If none is supplied then the current directory is used as the project root folder."
            };

        private Symbol[] CreateServeOptions()
        {
            string applicationPath = new FileInfo(typeof(Program).Assembly.Location).DirectoryName;
            
            return new Symbol[] {
                new Option(
                    new[]
                    {
                        "--server-uri",
                        "-s"
                    },
                    "The uri the server should use to listen on.",
                    new Argument<Uri>(new Uri("http://localhost:5678", UriKind.RelativeOrAbsolute))
                    {
                        Name = "uri"
                    }
                ),
                new Option(
                    new[]
                    {
                        "--templates",
                        "-t"
                    },
                    "The path that contains the project templates. " +
                    "If none is supplied then the default 'templates' folder is used.",
                    new Argument<string>(GetApplicationDataFolder("templates"))
                    {
                        Name = "templates"
                    }
                ),
                new Option(
                    new[]
                    {
                        "--www-root",
                        "-w"
                    },
                    "The path that contains the html resources for the web portal. " +
                    "If none is supplied then a 'wwwroot' sub-directory the tilde executable path is used.",
                    new Argument<string>(Path.Combine(applicationPath, "wwwroot"))
                    {
                        Name = "wwwroot"
                    }
                ),
                new Option(
                    new[]
                    {
                        "--no-logo",
                        "-n"
                    },
                    "Do not show tilde love logo.",
                    new Argument<bool>(false)
                    {
                        Name = "noLogo"
                    }
                )
            };
        }

        private int Serve(string projectFolder, Uri serverUri, string templates, string wwwRoot, bool noLogo)  
        {
            if (noLogo == false)
            {
                Logo.PrintLogo();
            }
            else 
            {
                Console.WriteLine("http://tilde.love"); 
            }
            
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                BuildWebHost(
                        new string[]
                        {
                            $"--{WebHostDefaults.ServerUrlsKey}", serverUri.ToString(),
                            $"--{WebHostDefaults.WebRootKey}", wwwRoot,
                            "--Tilde.ProjectFolder", projectFolder,
                            "--Tilde.ServerUri", serverUri.ToString(),
                            "--Tilde.Templates", templates,
                            "--Tilde.WwwRoot", wwwRoot
                        }
                    )
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return 1;
            }
            finally
            {
                AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            }
            
            return 0;
        }

        void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
            
        IWebHostBuilder BuildWebHost(string[] args)
        {
            string applicationPath = new FileInfo(typeof(Program).Assembly.Location).DirectoryName;

//            var configuration = new ConfigurationBuilder().C.AddCommandLine(args).Build();
            
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                // .SuppressStatusMessages(true) //disable the status messages
//                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.ServerUri)}", opts.ServerUri.ToString())
//                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.ProjectFolder)}", projectFolder)
//                .UseSetting($"{nameof(ServeVerb)}.{nameof(ServeVerb.WwwRoot)}", opts.WwwRoot)
                //.UseUrls(serverUri.ToString())
                //.UseWebRoot(wwwRoot)
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, true.ToString()) // add this line
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        IHostingEnvironment env = hostingContext.HostingEnvironment;
                        config
                            .AddJsonFile(Path.Combine(applicationPath, "appsettings.json"), true, true)
                            .AddJsonFile(Path.Combine(applicationPath, $"appsettings.{env.EnvironmentName}.json"), true, true);
//                        
//                        if (env.IsDevelopment())
//                        {
//                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
//                            if (appAssembly != null)
//                            {
//                                config.AddUserSecrets(appAssembly, optional: true);
//                            }
//                        }

                        config.AddEnvironmentVariables();
                        
                        config.AddCommandLine(args);
                    }
                )
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
    }
}
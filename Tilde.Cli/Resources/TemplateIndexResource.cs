// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Tilde.Core.Templates;

namespace Tilde.Cli.Resources
{
    public class TemplateIndexResource : CliResource
    {
        public static Argument<Uri> IndexUriArgument(IArgumentArity arity = null)
        {
            return new Argument<Uri>((Uri) null)
            {
                Arity = arity ?? ArgumentArity.ExactlyOne,
                Name = "index-uri",
                Description = "Template index uri."
            };
        }
        
        static Uri ToBase64IndexUri(Uri indexUri)
        {
            return new Uri(
                Convert.ToBase64String(Encoding.UTF8.GetBytes(indexUri.ToString())),
                UriKind.RelativeOrAbsolute
            );
        }

        public TemplateIndexResource()
        {
            Name = "index";
            Aliases.AddRange(new[] {"i"});
            Description = "Tilde project template index.";

            // add, register a template index uri,
            // remove, delete a template index uri,
            // pull, cache templates from a uri 
            // list, list all indexes 

            // pack, pack a local folder into a index 
            // unpack, unpack a index uri into a local folder

            VerbCommands[KnownVerbs.Add] = CreateNounCommand(
                "Add a template index uri.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                },
                IndexUriArgument(),
                CommandHandler.Create(
                    (Uri indexUri, Uri serverUri) => Add(indexUri, serverUri)
                )
            );

            VerbCommands[KnownVerbs.Remove] = CreateNounCommand(
                "Remove a template index uri.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                },
                IndexUriArgument(),
                CommandHandler.Create(
                    (Uri indexUri, Uri serverUri) => Remove(indexUri, serverUri)
                )
            );

            VerbCommands[KnownVerbs.Pull] = CreateNounCommand(
                "Pull an cache a template index uri.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                },
                IndexUriArgument(ArgumentArity.ZeroOrOne),
                CommandHandler.Create(
                    (Uri indexUri, Uri serverUri) => Pull(indexUri, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.List] = CreateNounCommand(
                "List template index uris.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                },
                null,
                CommandHandler.Create(
                    (Uri serverUri) => List(serverUri)
                )
            );

            Commands.Add(
                new Command(
                    "pack",
                    @"Create and pack a template index from a directory in the file system.",
                    null,
                    new Argument<string>
                    {
                        Arity = new ArgumentArity(1, 2),
                        Name = "paths",
                        Description = "Source path. Optional output path."
                    },
                    handler: CommandHandler.Create<List<string>>(
                        (paths) => Pack(paths)
                    )
                )
            );
            
            Commands.Add(
                new Command(
                    "unpack",
                    @"Unpack a template index to a directory in the file system.",
                    null,
                    new Argument<string>
                    {
                        Arity = new ArgumentArity(1, 2),
                        Name = "paths",
                        Description = "Source path. Optional output path."
                    },
                    handler: CommandHandler.Create<List<string>>(
                        (paths) => Unpack(paths)
                    )
                )
            );
        }

        private int List(Uri serverUri)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri(
                    $"api/1.0/indices",
                    UriKind.Relative
                )
            );

            try
            {
                string[] items;

                (HttpStatusCode statusCode, Dictionary<Uri, TemplateIndex> result) = RestApi
                    .GetResponse<Dictionary<Uri, TemplateIndex>>(
                        "GET",
                        requestUri
                    )
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        items = result.Keys.Select(u => u.ToString())
                            .ToArray();
                        break;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }

                Console.WriteLine(string.Join(Environment.NewLine, items));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;           
        }
        
        private int Pull(Uri indexUri, Uri serverUri)
        {
            Uri requestUri;

            if (indexUri == null)
            {
                requestUri = new Uri(
                    serverUri,
                    new Uri(
                        $"api/1.0/indices?pull=true",
                        UriKind.Relative
                    )
                );
            }
            else
            {
                requestUri = new Uri(
                    serverUri,
                    new Uri(
                        $"api/1.0/indices/{ToBase64IndexUri(indexUri)}?pull=true",
                        UriKind.Relative
                    )
                );
            }

            try
            {
                (HttpStatusCode statusCode, Dictionary<Uri, TemplateIndex> result) = RestApi
                    .GetResponse<Dictionary<Uri, TemplateIndex>>(
                        "GET",
                        requestUri
                    )
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        foreach (KeyValuePair<Uri, TemplateIndex> pair in result)
                        {
                            Console.WriteLine(pair.Key);
                            Console.WriteLine(JsonConvert.SerializeObject(pair.Value, Formatting.Indented));
                        }

                        break;
                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }

        private int Add(Uri indexUri, Uri serverUri)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri(
                    $"api/1.0/indices/{ToBase64IndexUri(indexUri)}",
                    UriKind.Relative
                )
            );

            try
            {
                (HttpStatusCode statusCode, Dictionary<Uri, TemplateIndex> result) = RestApi
                    .GetResponse<Dictionary<Uri, TemplateIndex>>(
                        "PUT",
                        requestUri
                    )
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        foreach (KeyValuePair<Uri, TemplateIndex> pair in result)
                        {
                            Console.WriteLine(pair.Key);
                            Console.WriteLine(JsonConvert.SerializeObject(pair.Value, Formatting.Indented));
                        }
                        break;
                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;    
        }

        private int Remove(Uri indexUri, Uri serverUri)
        {
            Uri requestUri = new Uri(
                serverUri,
                new Uri(
                    $"api/1.0/indices/{ToBase64IndexUri(indexUri)}",
                    UriKind.Relative
                )
            );

            try
            {
                (HttpStatusCode statusCode, Dictionary<Uri, TemplateIndex> result) = RestApi
                    .GetResponse<Dictionary<Uri, TemplateIndex>>(
                        "DELETE",
                        requestUri
                    )
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        foreach (KeyValuePair<Uri, TemplateIndex> pair in result)
                        {
                            Console.WriteLine(pair.Key);
                            Console.WriteLine(JsonConvert.SerializeObject(pair.Value, Formatting.Indented));
                        }
                        break;
                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;   
        }

        private int Pack(List<string> paths)
        {
            try
            {
                string source = paths.FirstOrDefault();
                string destination = paths.Count == 1 ? source : paths[1];

                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(source);
                DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(destination);

                TemplateIndex.Pack(sourceDirectoryInfo, destinationDirectoryInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }

                return -1;
            }

            return 0;
        }
        
        private int Unpack(List<string> paths)
        {
            try
            {
                string source = paths.FirstOrDefault();
                string destination = paths.Count == 1 ? source : paths[1];

                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(source);
                DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(destination);

                TemplateIndex.Unpack(sourceDirectoryInfo, destinationDirectoryInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                }

                return -1;
            }

            return 0;
        }
    }
}
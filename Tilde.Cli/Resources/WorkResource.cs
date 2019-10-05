using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tilde.Cli.Resources
{
    public class WorkResource : CliResource
    {
        public static Argument<Uri> WorkNameArgument()
        {
            return new Argument<Uri>
            {
                Arity = ArgumentArity.ExactlyOne,
                Name = "name",
                Description = "Name of a worker process."
            };
        }
        
        public static Option OptionalWorkNameOption()
        {
            return new Option(
                new[]
                {
                    "--name",
                    "-n"
                },
                "Name of a worker process.",
                new Argument<Uri>()
                {
                    Name = "name"
                }
            );
        }
        
        public static Argument<Uri> OptionalWorkNameArgument()
        {
            return new Argument<Uri>
            {
                Arity = ArgumentArity.ZeroOrOne,
                Name = "name",
                Description = "Name of a worker process."
            };
        }
        
        public WorkResource()
        {
            Name = "work";
            Aliases.AddRange(new[] {"process", "w"});
            Description = "A process that is being managed by tilde.";

            VerbCommands[KnownVerbs.Delete] = CreateNounCommand(
                "Delete a tilde project file.",
                new[] {
                    CommonArguments.ServerUriOption(),
                },
                WorkNameArgument(),
                CommandHandler.Create(
                    (Uri name, Uri serverUri) => Delete(name, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.New] = CreateNounCommand(
                "Create a worker for a project.",
                new[]
                {
                    CommonArguments.ServerUriOption(),
                    OptionalWorkNameOption()
                },
                CommonArguments.ProjectNameArgument(),
                CommandHandler.Create(
                    (Uri serverUri, Uri project, Uri name) => New(project, name, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.Get] = CreateNounCommand(
                "Gets a worker.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                WorkNameArgument(),
                CommandHandler.Create(
                    (Uri serverUri, Uri name) => Get(name, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.Set] = CreateNounCommand(
                "Sets a worker.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                WorkNameArgument(),
                CommandHandler.Create(
                    (Uri serverUri, Uri name) => Set(name, serverUri)
                )
            );

            VerbCommands[KnownVerbs.List] = CreateNounCommand(
                "List all workers.",
                new[]
                {
                    CommonArguments.ServerUriOption()
                },
                null,
                CommandHandler.Create(
                    (Uri serverUri) => List(serverUri)
                )
            );

            VerbCommands[KnownVerbs.Start] = CreateNounCommand(
                "Start a worker process.",
                new[] {
                    CommonArguments.ServerUriOption(),
                },
                WorkNameArgument(),
                CommandHandler.Create(
                    (Uri name, Uri serverUri) => Start(name, serverUri)
                )
            );
            
            VerbCommands[KnownVerbs.Stop] = CreateNounCommand(
                "Stops a worker process.",
                new[] {
                    CommonArguments.ServerUriOption(),
                },
                WorkNameArgument(),
                CommandHandler.Create(
                    (Uri name, Uri serverUri) => Stop(name, serverUri)
                )
            );
        }

        private int New(Uri project, Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name ?? project}/from-project/{project}", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("GET", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name ?? project} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int List(Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("GET", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Endpoint not found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int Set(Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name}", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("PUT", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int Start(Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name}/start", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("GET", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int Stop(Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name}/stop", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("GET", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int Get(Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name}", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("GET", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }

        private int Delete(Uri name, Uri serverUri)
        {
            try
            {
                Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/work/{name}", UriKind.Relative));

                (HttpStatusCode statusCode, string body) = RestApi.GetResponse("DELETE", requestUri).Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(JToken.Parse(body).ToString(Formatting.Indented));
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Worker {name} could not be found.");
                        return 0;

                    default:
                        Console.WriteLine($"Unexpected status: {statusCode}");
                        return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);

                return -1;
            }
        }
    }
}
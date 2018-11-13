using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json;
using Tilde.Core.Controls;

namespace Tilde.Cli
{
    public static class Commands
    {
        public static IEnumerable<Type> Verbs => new[]
        {
            typeof(ListVerb),
            typeof(NewVerb),
            typeof(DeleteVerb),
            typeof(StartVerb),
            typeof(StopVerb),
            typeof(PullVerb),
            typeof(PushVerb),
            typeof(WatchVerb),
            typeof(LogsVerb)
        };

        public static int Delete(DeleteVerb opts)
        {
            // Logo.PrintLogo(0, 3);

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative));

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = "DELETE";

                //Console.WriteLine($"{request.Method} {requestUri}");

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponseAsync()
                    .Result)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            Console.WriteLine($"Deleted project {opts.Project}");

                            List(new ListVerb {ServerUri = opts.ServerUri, Type = ListableType.projects});
                            return 0;

                        default:
                            Console.WriteLine($"Unexpected status: {response.StatusCode}");

                            return -1;
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.Flatten()
                    .InnerExceptions)
                {
                    if (e is WebException wex)
                    {
                        HttpStatusCode statusCode = (wex.Response as HttpWebResponse).StatusCode;

                        switch (statusCode)
                        {
                            case HttpStatusCode.NotFound:
                                Console.WriteLine($"Project {opts.Project} does not exist.");
                                return 0;
                            default:
                                Console.WriteLine($"Unexpected status: {statusCode}");
                                return -1;
                        }
                    }

                    Console.WriteLine("Could not contact the server.");
                    Console.WriteLine(e.Message);
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }

        public static int List(ListVerb opts)
        {
            // PrintLogo(0, 4);

            try
            {
                Uri requestUri;

                switch (opts.Type)
                {
                    case ListableType.projects:
                        requestUri = new Uri(opts.ServerUri, new Uri("api/1.0/projects", UriKind.Relative));
                        break;

                    case ListableType.files:
                    case ListableType.controls:
                        requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative));
                        break;

                    case ListableType.clients:
                        requestUri = new Uri(opts.ServerUri, new Uri("api/1.0/clients", UriKind.Relative));
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = "GET";

                //Console.WriteLine($"{request.Method} {requestUri}");

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponseAsync()
                    .Result)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            break;

                        default:
                            Console.WriteLine($"Unexpected status: {response.StatusCode}");

                            return -1;
                    }

                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        string[] items;

                        switch (opts.Type)
                        {
                            case ListableType.projects:
                                Dictionary<Uri, ProjectResult> projects = JsonConvert.DeserializeObject<Dictionary<Uri, ProjectResult>>(
                                    strReader.ReadToEndAsync()
                                        .Result
                                );

                                items = projects.Keys.Select(u => u.ToString())
                                    .ToArray();
                                break;

                            case ListableType.files:
                            case ListableType.controls:

                                ProjectResult project = JsonConvert.DeserializeObject<ProjectResult>(
                                    strReader.ReadToEndAsync()
                                        .Result
                                );

                                if (opts.Type == ListableType.files)
                                {
                                    items = project.Files.Select(u => u.ToString())
                                        .ToArray();
                                }
                                else if (opts.Type == ListableType.controls)
                                {
                                    items = project.Controls.Sources.Values.Select(u => u.ToString())
                                        .ToArray();
                                }
                                else
                                {
                                    items = new string[0];
                                }

                                break;

                            case ListableType.clients:
                                items = new string[0];
                                break;

                            default: throw new ArgumentOutOfRangeException();
                        }

                        Console.WriteLine(string.Join(Environment.NewLine, items));
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.Flatten()
                    .InnerExceptions)
                {
                    if (e is WebException wex && wex.Response is HttpWebResponse response)
                    {
                        HttpStatusCode statusCode = response.StatusCode;

                        switch (statusCode)
                        {
                            case HttpStatusCode.NotFound:
                                Console.WriteLine($"Project {opts.Project} does not exist.");
                                return 0;
                            default:
                                Console.WriteLine($"Unexpected status: {statusCode}");
                                return -1;
                        }
                    }

                    Console.WriteLine("Could not contact the server.");
                    Console.WriteLine(e.Message);
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }

        public static int Logs(LogsVerb opts)
        {
            try
            {
                if (GetRawFile(opts, "build") == false)
                {
                    return -1;
                }

                if (GetRawFile(opts, "log") == false)
                {
                    return -1;
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.Flatten()
                    .InnerExceptions)
                {
                    if (e is WebException wex)
                    {
                        HttpStatusCode statusCode = (wex.Response as HttpWebResponse).StatusCode;

                        switch (statusCode)
                        {
                            case HttpStatusCode.NotFound:
                                Console.WriteLine($"Project {opts.Project} not found.");
                                return 0;

                            default:
                                Console.WriteLine($"Unexpected status: {statusCode}");
                                return -1;
                        }
                    }

                    Console.WriteLine("Could not contact the server.");
                    Console.WriteLine(e.Message);
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not contact the server.");
                Console.WriteLine(e.Message);
                return -1;
            }

            return 0;
        }

        public static int New(NewVerb opts)
        {
            Logo.PrintLogo();

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/projects/{opts.Project}", UriKind.Relative));

                HttpStatusCode statusCode = DoRequest("POST", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.Created:
                        Console.WriteLine($"Created project {opts.Project}");
                        List(new ListVerb {ServerUri = opts.ServerUri, Type = ListableType.projects});
                        return 0;

                    case HttpStatusCode.Conflict:
                        Console.WriteLine($"Project {opts.Project} already exists.");
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

        public static void Parse(ParserResult<object> parserResult)
        {
            parserResult.WithParsed((ListVerb opts) => List(opts));

            parserResult.WithParsed((NewVerb opts) => New(opts));
            parserResult.WithParsed((DeleteVerb opts) => Delete(opts));

            parserResult.WithParsed((StartVerb opts) => Start(opts));
            parserResult.WithParsed((StopVerb opts) => Stop(opts));

            parserResult.WithParsed((PullVerb opts) => Pull(opts));
            parserResult.WithParsed((PushVerb opts) => Push(opts));

            parserResult.WithParsed((WatchVerb opts) => Watch(opts));

            parserResult.WithParsed((LogsVerb opts) => Logs(opts));
        }

        public static void ParseErrors(ParserResult<object> parserResult)
        {
            parserResult.WithNotParsed(
                errs =>
                {
                    string helpText = string.Empty;

                    List<ErrorType> errorTypes = errs.Select(e => e.Tag)
                        .ToList();

                    if (parserResult.TypeInfo.Current == typeof(NullInstance))
                    {
                        if (errorTypes.Contains(ErrorType.NoVerbSelectedError))
                        {
                            helpText = HelpText.AutoBuild(
                                    parserResult,
                                    h =>
                                    {
                                        // Configure HelpText here  or create your own and return it 
                                        h.AdditionalNewLineAfterOption = false;
                                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                    },
                                    e => e
                                )
                                .ToString();
                        }
                        else
                        {
                            helpText = HelpText.AutoBuild(
                                    parserResult,
                                    h =>
                                    {
                                        // Configure HelpText here  or create your own and return it 
                                        h.AdditionalNewLineAfterOption = false;
                                        return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                    },
                                    e => e
                                )
                                .ToString();
                        }
                    }
                    else
                    {
                        helpText = HelpText.AutoBuild(
                                parserResult,
                                h =>
                                {
                                    // Configure HelpText here  or create your own and return it 
                                    h.AdditionalNewLineAfterOption = false;
                                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                                },
                                e => e
                            )
                            .ToString();
                    }
//                    Console.Error.WriteLine(parserResult.Tag); 
//                    Console.Error.WriteLine(parserResult.TypeInfo.Current);
//                    Console.Error.WriteLine(string.Join(", ", parserResult.TypeInfo.Choices)); 
//                    
//                    foreach (var e in errs)
//                    {
//                        Console.Error.WriteLine(e.Tag);
//                        Console.Error.WriteLine(e.StopsProcessing);                        
//                    }

                    string[] lines = helpText
                        .Split(
                            new[] {Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries
                        );

                    Console.Error.Write(string.Join(Environment.NewLine, lines));
                }
            );
        }

        public static int Pull(PullVerb opts)
        {
            // Logo.PrintLogo(0, 3);

            return 0;
        }

        public static int Push(PushVerb opts)
        {
            // Logo.PrintLogo(0, 3);

            return 0;
        }

        public static int Start(StartVerb opts)
        {
            // Logo.PrintLogo(0, 3);

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/script/runtime/run/{opts.Project}", UriKind.Relative));

                HttpStatusCode statusCode = DoRequest("GET", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Started project {opts.Project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {opts.Project} not found.");
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

        public static int Stop(StopVerb opts)
        {
            // Logo.PrintLogo(0, 3);

            try
            {
                Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/script/runtime/stop", UriKind.Relative));

                HttpStatusCode statusCode = DoRequest("GET", requestUri)
                    .Result;

                switch (statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine($"Stopped project {opts.Project}");
                        return 0;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"Project {opts.Project} not found.");
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

        public static int Watch(WatchVerb opts)
        {
            Logo.PrintLogo(0, 3);

            return 0;
        }

        private static async Task<HttpStatusCode> DoRequest(string method, Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                //Console.WriteLine($"{request.Method} {requestUri}");

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    return response.StatusCode;
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae
                    .Flatten()
                    .InnerExceptions)
                {
                    if (e is WebException == false)
                    {
                        continue;
                    }

                    WebException wex = (WebException) e;

                    if (!(wex.Response is HttpWebResponse response))
                    {
                        continue;
                    }

                    Console.WriteLine("Could not contact the server.");
                    Console.WriteLine(e.Message);

                    HttpStatusCode statusCode = response.StatusCode;

                    return statusCode;
                }

                throw;
            }
        }

        private static bool GetRawFile(LogsVerb opts, string file)
        {
            Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/files/{opts.Project}/{file}", UriKind.Relative));

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

            request.Method = WebRequestMethods.Http.Get;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponseAsync()
                .Result)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    default:
                        Console.WriteLine($"Unexpected Status: {response.StatusCode}");

                    {
                        return false;
                    }
                }

                using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                {
                    Console.WriteLine(strReader.ReadToEnd());
                }
            }

            return true;
        }
//        public static IEnumerable<Error> OnlyMeaningfulOnes(this IEnumerable<Error> errors)
//        {
//            return errors
//                .Where(e => !e.StopsProcessing)
//                .Where(e => !(e.Tag == ErrorType.UnknownOptionError
//                              && ((UnknownOptionError)e).Token .Equals("help", StringComparison.OrdinalIgnoreCase)));
//        }
//        
//        /// <summary>
//        /// Creates a new instance of the <see cref="CommandLine.Text.HelpText"/> class using common defaults.
//        /// </summary>
//        /// <returns>
//        /// An instance of <see cref="CommandLine.Text.HelpText"/> class.
//        /// </returns>
//        /// <param name='parserResult'>The <see cref="CommandLine.ParserResult{T}"/> containing the instance that collected command line arguments parsed with <see cref="CommandLine.Parser"/> class.</param>
//        /// <param name='onError'>A delegate used to customize the text block of reporting parsing errors text block.</param>
//        /// <param name='onExample'>A delegate used to customize <see cref="CommandLine.Text.Example"/> model used to render text block of usage examples.</param>
//        /// <param name="verbsIndex">If true the output style is consistent with verb commands (no dashes), otherwise it outputs options.</param>
//        /// <param name="maxDisplayWidth">The maximum width of the display.</param>
//        /// <remarks>The parameter <paramref name="verbsIndex"/> is not ontly a metter of formatting, it controls whether to handle verbs or options.</remarks>
//        public static HelpText AutoBuild<T>(
//            ParserResult<T> parserResult,
//            Func<HelpText, HelpText> onError,
//            Func<Example, Example> onExample,
//            bool verbsIndex = false,
//            int maxDisplayWidth = 80)
//        {
//            var auto = new HelpText
//            {
//                Heading = HeadingInfo.Empty,
//                Copyright = CopyrightInfo.Empty,
//                AdditionalNewLineAfterOption = true,
//                AddDashesToOption = !verbsIndex,
//                MaximumDisplayWidth = maxDisplayWidth
//            };
//
//            try
//            {
//                auto.Heading = HeadingInfo.Default;
//                auto.Copyright = CopyrightInfo.Default;
//            }
//            catch (Exception)
//            {
//                auto = onError(auto);
//            }
//
//            var errors = Enumerable.Empty<Error>();
//
//            if (onError != null && parserResult.Tag == ParserResultType.NotParsed)
//            {
//                errors = ((NotParsed<T>)parserResult).Errors;
//
//                if (errors.OnlyMeaningfulOnes().Any())
//                    auto = onError(auto);
//            }
//
//            ReflectionHelper.GetAttribute<AssemblyLicenseAttribute>()
//                .Do(license => license.AddToHelpText(auto, true));
//
//            var usageAttr = ReflectionHelper.GetAttribute<AssemblyUsageAttribute>();
//            var usageLines = HelpText.RenderUsageTextAsLines(parserResult, onExample).ToMaybe();
//
//            if (usageAttr.IsJust() || usageLines.IsJust())
//            {
//                var heading = auto.SentenceBuilder.UsageHeadingText();
//                if (heading.Length > 0)
//                    auto.AddPreOptionsLine(heading);
//            }
//
//            usageAttr.Do(
//                usage => usage.AddToHelpText(auto, true));
//
//            usageLines.Do(
//                lines => auto.AddPreOptionsLines(lines));
//
//            if ((verbsIndex && parserResult.TypeInfo.Choices.Any())
//                || errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
//            {
//                auto.AddDashesToOption = false;
//                auto.AddVerbs(parserResult.TypeInfo.Choices.ToArray());
//            }
//            else
//                auto.AddOptions(parserResult);
//
//            return auto;
//        }

        private class ProjectResult
        {
            [JsonProperty("controls")] public ControlGroup Controls;

            [JsonProperty("files")] public List<Uri> Files;
            [JsonProperty("uri")] public Uri Uri;
        }
    }
}
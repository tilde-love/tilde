// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tilde.Cli.Verbs;

namespace Tilde.Cli
{       
    public class Verb
    {
        public static bool GetRawFile(LogsVerb opts, string file)
        {
            Uri requestUri = new Uri(opts.ServerUri, new Uri($"api/1.0/Files/{opts.Project}/{file}", UriKind.Relative));

            Tuple<HttpStatusCode, string> response = GetResponse("GET", requestUri).Result; 

            switch (response.Item1)
            {
                case HttpStatusCode.OK:
                    Console.WriteLine(response.Item2);
                    return true;
                
                case HttpStatusCode.NotFound:
                    Console.WriteLine($"File {file} not found.");
                    return true;
                
                default:
                    Console.WriteLine($"Unexpected Status: {response.Item1}");
                    return false;
            }                       
        }

        public static async Task<Tuple<HttpStatusCode, string>> GetResponse(string method, Uri requestUri) 
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        return new Tuple<HttpStatusCode, string>(
                            response.StatusCode,
                            await strReader.ReadToEndAsync()
                        );
                    }
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

                    return new Tuple<HttpStatusCode, string>(statusCode, default(string));
                }

                throw;
            }
        }
        
        public static async Task<Tuple<HttpStatusCode, T>> GetResponse<T>(string method, Uri requestUri) 
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        return new Tuple<HttpStatusCode, T>(
                            response.StatusCode,
                            JsonConvert.DeserializeObject<T>(
                                await strReader.ReadToEndAsync()
                            )
                        );
                    }
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

                    return new Tuple<HttpStatusCode, T>(statusCode, default(T));
                }

                throw;
            }
        }

        public static async Task<HttpStatusCode> MakeRequest(string method, Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

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
    }
}
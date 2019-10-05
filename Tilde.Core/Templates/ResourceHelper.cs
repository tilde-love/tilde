// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tilde.Core.Templates
{
    public static class ResourceHelper
    {
        public static byte[] GetResourceBytes(Uri resourceUri)
        {
            if (resourceUri.IsFile == true)
            {
                return File.ReadAllBytes(resourceUri.LocalPath); 
            }
            else
            {
                (HttpStatusCode statusCode, byte[] bytes) = GetResponseBytes(resourceUri);

                if (statusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Unexpected status code {statusCode}");
                }

                return bytes;
            }
        }
        
        public static T GetResource<T>(Uri resourceUri)
        {
            if (resourceUri.IsFile == true)
            {
                using (StreamReader file = File.OpenText(resourceUri.LocalPath))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    return (T) serializer.Deserialize(file, typeof(T));
                }
            }
            else
            {
                (HttpStatusCode statusCode, T result) = GetResponse<T>(resourceUri);

                if (statusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Unexpected status code {statusCode}");
                }

                return result;
            }
        }
        
        public static (HttpStatusCode statusCode, T result) GetResponse<T>(Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        return (
                            response.StatusCode,
                            JsonConvert.DeserializeObject<T>(
                                strReader.ReadToEnd()
                            )
                        );
                    }
                }
            }
            catch (WebException wex)
            {
                if (!(wex.Response is HttpWebResponse response))
                {
                    throw;
                }

                HttpStatusCode statusCode = response.StatusCode;

                if (statusCode == HttpStatusCode.NotFound)
                {
                    return (statusCode, default);
                }

                throw;
            }
        }
        
        private static (HttpStatusCode statusCode, byte[] bytes) GetResponseBytes(Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (Stream stream = response.GetResponseStream())
                    {
                        stream.CopyTo(ms);

                        return (
                            response.StatusCode,
                            ms.ToArray()
                        );
                    }
                }
            }
            catch (WebException wex)
            {
                if (!(wex.Response is HttpWebResponse response))
                {
                    throw;
                }

                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(wex.Message);

                HttpStatusCode statusCode = response.StatusCode;

                return (statusCode, default);
            }
        }
    }
}
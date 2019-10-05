// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tilde.Cli
{
    public static class RestApi
    {
        public static void DeleteFile(Uri serverUri, string project, string file)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{project}/{file}", UriKind.Relative));

            try
            {
                (HttpStatusCode statusCode, string body) response = GetResponse("DELETE", requestUri)
                    .Result;

                switch (response.statusCode)
                {
                    case HttpStatusCode.OK:
                        return;

                    default:
                        throw new Exception($"Unexpected Status: {response.statusCode}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not contact the server. {requestUri}", e);
            }
        }

        public static void DownloadFile(Uri serverUri, string projectFile, string localFile)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{projectFile}", UriKind.Relative));

            byte[] bytes = null;

            try
            {
                (HttpStatusCode statusCode, byte[] bytes) response = GetResponseBytes("GET", requestUri)
                    .Result;

                switch (response.statusCode)
                {
                    case HttpStatusCode.OK:
                        bytes = response.bytes;
                        break;

                    case HttpStatusCode.NotFound:
                        throw new Exception($"File {projectFile} not found.");

                    default:
                        throw new Exception($"Unexpected Status: {response.statusCode}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not contact the server. {requestUri}", e);
            }

            FileInfo fileInfo = new FileInfo(localFile);

            if (fileInfo.Directory?.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            File.WriteAllBytes(localFile, bytes);
        }

        public static bool GetRawFile(Uri serverUri, string project, string file)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{project}/{file}", UriKind.Relative));

            try
            {
                (HttpStatusCode statusCode, string body) response = GetResponse("GET", requestUri)
                    .Result;

                switch (response.statusCode)
                {
                    case HttpStatusCode.OK:
                        Console.WriteLine(response.body);
                        return true;

                    case HttpStatusCode.NotFound:
                        Console.WriteLine($"File {file} not found.");
                        return true;

                    default:
                        Console.WriteLine($"Unexpected Status: {response.statusCode}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not contact the server. {requestUri}");
                Console.WriteLine(e.Message);
                return false;
            }
        }
        
        public static async Task<(HttpStatusCode statusCode, string body)> GetResponse(string method, Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        return (
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

                    Console.WriteLine($"Could not contact the server. {requestUri}");
                    Console.WriteLine(e.Message);

                    HttpStatusCode statusCode = response.StatusCode;

                    return (statusCode, default);
                }

                throw;
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

                if (statusCode == HttpStatusCode.NotFound)
                {
                    return (statusCode, default);
                }

                throw;
            }
        }

        public static async Task<(HttpStatusCode statusCode, T result)> GetResponse<T>(string method, Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                    {
                        return (
                            response.StatusCode,
                            JsonConvert.DeserializeObject<T>(await strReader.ReadToEndAsync()
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

                    HttpStatusCode statusCode = response.StatusCode;

                    if (statusCode == HttpStatusCode.NotFound)
                    {
                        return (statusCode, default);
                    }

                    Console.WriteLine($"Could not contact the server. {requestUri}");
                    Console.WriteLine(e.Message);
                }

                throw;
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

                if (statusCode == HttpStatusCode.NotFound)
                {
                    return (statusCode, default);
                }

                throw;
            }
        }

        public static async Task<(HttpStatusCode statusCode, byte[] bytes)> GetResponseBytes(string method, Uri requestUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);

                request.Method = method;

                using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (Stream stream = response.GetResponseStream())
                    {
                        await stream.CopyToAsync(ms);

                        return (
                            response.StatusCode,
                            ms.ToArray()
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

                    Console.WriteLine($"Could not contact the server. {requestUri}");
                    Console.WriteLine(e.Message);

                    HttpStatusCode statusCode = response.StatusCode;

                    return (statusCode, default);
                }

                throw;
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

                if (statusCode == HttpStatusCode.NotFound)
                {
                    return (statusCode, default);
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

                    Console.WriteLine($"Could not contact the server. {requestUri}");
                    Console.WriteLine(e.Message);

                    HttpStatusCode statusCode = response.StatusCode;

                    return statusCode;
                }

                throw;
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

                if (statusCode == HttpStatusCode.NotFound)
                {
                    return statusCode;
                }

                throw;
            }
        }

        public static async Task<(HttpStatusCode statusCode, string body)> PostFile(Uri url, string name, string filePath)
        {
            string rn = "\r\n";

            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes($"{rn}--{boundary}{rn}");
            byte[] endBoundaryBytes = Encoding.ASCII.GetBytes($"{rn}--{boundary}--");

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.ContentType = $"multipart/form-data; boundary={boundary}";
            request.Method = "POST";
            request.KeepAlive = true;

            Stream memStream = new MemoryStream();

//            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
//        
//            if (formFields != null)
//            {
//                foreach (string key in formFields.Keys)
//                {
//                    string formItem = string.Format(formdataTemplate, key, formFields[key]);
//                    byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(formItem);
//                    
//                    memStream.Write(formItemBytes, 0, formItemBytes.Length);
//                }
//            }

            string headerTemplate =
                "Content-Disposition: form-data; " +
                $@"name=""{{0}}""; filename=""{{1}}""{rn}" +
                $"Content-Type: application/octet-stream{rn}{rn}";

            memStream.Write(boundaryBytes, 0, boundaryBytes.Length);

            string header = string.Format(headerTemplate, "file", name);
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            memStream.Write(headerBytes, 0, headerBytes.Length);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];
                int bytesRead = 0;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    memStream.Write(buffer, 0, bytesRead);
                }
            }

            memStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            request.ContentLength = memStream.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                memStream.Position = 0;
                byte[] tempBuffer = new byte[memStream.Length];
                memStream.Read(tempBuffer, 0, tempBuffer.Length);
                memStream.Close();
                requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
            {
                using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                {
                    return (
                        response.StatusCode,
                        await strReader.ReadToEndAsync()
                    );
                }
            }
        }

        public static async Task<(HttpStatusCode statusCode, string body)> PutFile(Uri url, string filePath)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.ContentType = "application/octet-stream";
            request.Method = "PUT";
            request.KeepAlive = true;

            using (Stream requestStream = request.GetRequestStream())
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(requestStream);
            }

            using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
            {
                using (StreamReader strReader = new StreamReader(response.GetResponseStream()))
                {
                    return (
                        response.StatusCode,
                        await strReader.ReadToEndAsync()
                    );
                }
            }
        }

        public static void UploadFile(
            Uri serverUri,
            string project,
            string file,
            string filePath)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{project}/{file}", UriKind.Relative));

            try 
            {
                (HttpStatusCode statusCode, string body) response = PutFile(requestUri, filePath)
                    .Result;

                switch (response.statusCode)
                {
                    case HttpStatusCode.Created:
                        return;

                    default:
                        throw new Exception($"Unexpected Status: {response.statusCode}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not contact the server. {requestUri}", e);
            }
        }
        
        public static void UploadFile(
            Uri serverUri,
            string file,
            string filePath)
        {
            Uri requestUri = new Uri(serverUri, new Uri($"api/1.0/projects/{file}", UriKind.Relative));

            try 
            {
                (HttpStatusCode statusCode, string body) response = PutFile(requestUri, filePath)
                    .Result;

                switch (response.statusCode)
                {
                    case HttpStatusCode.Created:
                        return;

                    default:
                        throw new Exception($"Unexpected Status: {response.statusCode}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not contact the server. {requestUri}", e);
            }
        }
    }
}
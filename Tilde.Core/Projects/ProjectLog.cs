// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;

namespace Tilde.Core.Projects
{
    public enum LogType
    {
        Runtime,
        Build
    }

    public class ProjectLog : IDisposable
    {
        private static readonly object syncRoot = new object();
        private readonly LogType logType;

        private readonly FileStream stream;
        private readonly StreamWriter writer;

        public string FullName { get; }

        public ProjectLog(Project project, LogType logType)
        {
            this.logType = logType;

            FullName = Path.Combine(project.ProjectFolder.FullName, logType == LogType.Runtime ? "log" : "build");

            lock (syncRoot)
            {
                if (File.Exists(FullName))
                {
                    File.Delete(FullName);
                }
            }

            stream = new FileStream(FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(stream);
        }
        
        public ProjectLog(string fullName, LogType logType)
        {
            this.logType = logType;

            FullName = fullName; 

            lock (syncRoot)
            {
                if (File.Exists(FullName))
                {
                    File.Delete(FullName);
                }
            }

            stream = new FileStream(FullName, FileMode.Create, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(stream);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            writer.Flush();
            writer?.Dispose();
            stream?.Dispose();
        }

        public void Log(string message)
        {
            try
            {
                lock (syncRoot)
                {
                    writer.WriteLine(message);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(message);

                Console.WriteLine(ex.ToString());
            }
        }

        public string Read()
        {
            lock (syncRoot)
            {
                try
                {
                    if (File.Exists(FullName) == false)
                    {
                        return string.Empty;
                    }

                    using (FileStream fileStream = new FileStream(FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            }
        }
    }
}
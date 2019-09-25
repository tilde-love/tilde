// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Runtime.Dotnet
{
//    public class ProjectLauncher : IDisposable
//    {
//        private Process process;
//
//        private readonly Project project;
//
//        public ProjectLog Log { get; }
//
//        public ProjectCompileSettings Settings { get; }
//
//        public ProjectLauncher(Project project, ProjectCompileSettings settings)
//        {
//            this.project = project;
//
//            Log = new ProjectLog(project, LogType.Runtime);
//
//            Settings = settings;
//        }
//
//        public void Dispose()
//        {
//            if (process != null)
//            {
//                Debug.WriteLine("Shutting down script");
//                process.StandardInput.WriteLine(ModuleCommand.Exit.ToString());
//
//                if (process.WaitForExit(10000) == false)
//                {
//                    Debug.WriteLine("Shutdown failed killing process");
//                    process.Kill();
//                }
//
//                process.OutputDataReceived -= ProcessOnOutputDataReceived;
//                process.ErrorDataReceived -= ProcessOnErrorDataReceived;
//
//                process?.Dispose();
//                Debug.WriteLine("Done!");
//
//                process = null;
//            }
//
//            Log?.Dispose();
//        }
//
//        public void Launch(Uri serverUri)
//        {
////            string serverHost = Environment
////                                    .GetEnvironmentVariable("ASPNETCORE_URLS")
////                                    ?.Replace("*","localhost") 
////                                ?? throw new Exception("Cannot resolve host uri"); 
//
//            ProcessStartInfo startInfo = new ProcessStartInfo
//            {
//                FileName = "dotnet",
//                Arguments = $@"""bin/{project.Uri}.dll"" {new Uri(serverUri, "api/module")} ""{project.Uri}""",
//                UseShellExecute = false,
//                CreateNoWindow = true,
//                RedirectStandardOutput = true,
//                RedirectStandardError = true,
//                RedirectStandardInput = true,
//                WorkingDirectory = project.ProjectFolder.FullName
//            };
//
//            // startInfo.Environment["COREHOST_TRACE"] = "1"; 
//
//            process = new Process
//            {
//                StartInfo = startInfo,
//                EnableRaisingEvents = true
//            };
//
//            process.OutputDataReceived += ProcessOnOutputDataReceived;
//            process.ErrorDataReceived += ProcessOnErrorDataReceived;
//
//            if (process.Start() == false)
//            {
//                throw new Exception("Project failed to start.");
//            }
//
//            process.BeginOutputReadLine();
//            process.BeginErrorReadLine();
//        }
//
//        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            Log.Log(e.Data);
//        }
//
//        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            Log.Log(e.Data);
//        }
//    }
}
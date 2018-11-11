// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Tilde.Core.Projects;

namespace Tilde.Runtime.Dotnet
{
    public class ProjectBuilder
    {
        private static readonly Regex ErrorRegex = new Regex(
            @"(?<name>.*)\((?<line>\d*),(?<column>\d*)\): (?<type>error|warning) (?<code>\w+): (?<message>.*) \[(?<path>.+)\]",
            RegexOptions.Compiled
        );

        private readonly Project project;

        public ProjectLog Build { get; }

        public bool Compiled { get; private set; }

        public ProjectCompileSettings Settings { get; }

        public ProjectBuilder(Project project, ProjectCompileSettings settings)
        {
            this.project = project;
            Build = new ProjectLog(project, LogType.Build);
            Settings = settings;
        }

        public ProjectCompileResult Compile()
        {
            Compiled = false;

            // generate csproj file 

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "build -v m --packages ../../.packages -o bin",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WorkingDirectory = project.ProjectFolder.FullName
            };

            ProjectCompileResult result = new ProjectCompileResult {Errors = new Dictionary<Uri, List<Error>>()};

            using (Process process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            })
            {
                bool success = false;
                bool buildFailed = false;

                List<string> allErrors = new List<string>();

                void Output(object o, DataReceivedEventArgs args)
                {
                    string message = args.Data;

                    switch (message)
                    {
                        case null: return;
                        case "Build FAILED.":
                            buildFailed = true;
                            break;
                        case "Build succeeded.":
                            success = true;
                            break;
                        default:
                        {
                            if (ErrorRegex.IsMatch(message)
                                && allErrors.Contains(message) == false)
                            {
                                allErrors.Add(message);
                            }

                            break;
                        }
                    }

                    Build.Log(args.Data);
                }

                process.OutputDataReceived += Output;
                process.ErrorDataReceived += Output;

                if (process.Start() == false)
                {
                    throw new Exception("Project failed to start.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.OutputDataReceived -= Output;
                process.ErrorDataReceived -= Output;

                // Build FAILED.

                foreach (string message in allErrors)
                {
                    Match match = ErrorRegex.Match(message);

                    Uri file = new Uri(
                        match.Groups["name"]
                            .Value,
                        UriKind.RelativeOrAbsolute
                    );

                    if (result.Errors.TryGetValue(file, out List<Error> list) == false)
                    {
                        list = new List<Error>();

                        result.Errors[file] = list;
                    }

                    int column = int.Parse(
                        match.Groups["column"]
                            .Value
                    );

                    int line = int.Parse(
                                   match.Groups["line"]
                                       .Value
                               ) - 1;

                    Error error = new Error
                    {
                        Type = match.Groups["type"]
                            .Value.ToLowerInvariant(),
                        Text = match.Groups["message"]
                            .Value,
                        Span = new ErrorSpan
                        {
                            StartColumn = column, StartLine = line, EndColumn = column, EndLine = line
                        }
                    };

                    list.Add(error);
                }

                Compiled = success && buildFailed == false;
            }

            Build?.Dispose();

            return result;
        }
    }
}
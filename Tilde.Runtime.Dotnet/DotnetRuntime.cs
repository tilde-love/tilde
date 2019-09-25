// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Tilde.Core;
using Tilde.Core.Projects;

namespace Tilde.Runtime.Dotnet
{
//    public class DotnetRuntime : IRuntime
//    {
//        private Project activeProject;
//        private ProjectLauncher projectLauncher;
//        private readonly Uri serverUri;
//        private RuntimeState state = RuntimeState.Running;
//        private readonly object syncRoot = new object();
//        private int updated;
//
//        public DotnetRuntime(Uri serverUri)
//        {
//            this.serverUri = serverUri;
//        }
//
//        /// <inheritdoc />
//        public void Dispose()
//        {
//            lock (syncRoot)
//            {
//                ProjectLauncher launcher = Interlocked.Exchange(ref this.projectLauncher, null);
//
//                launcher?.Dispose();
//
//                activeProject = null;
//            }
//        }
//
//        /// <inheritdoc />
//        public void Load(Project project)
//        {
//            Debug.WriteLine($"Load project: {project.Uri}");
//
//            Project last = Interlocked.Exchange(ref activeProject, project);
//
//            if (last != null)
//            {
//                last.WatchFilesChanged -= OnWatchFilesChanged;
//            }
//
//            project.WatchFilesChanged += OnWatchFilesChanged;
//
//            Updated();
//        }
//
//        /// <inheritdoc />
//        public Project Project => activeProject;
//
//        /// <inheritdoc />
//        public event ProjectEvent ProjectChanged;
//
//        /// <inheritdoc />
//        public RuntimeState State
//        {
//            get => state;
//            set
//            {
//                state = value;
//
//                StateChanged?.Invoke(this, EventArgs.Empty);
//
//                if (state == RuntimeState.Stopped)
//                {
//                    ProjectLauncher launcher = Interlocked.Exchange(ref this.projectLauncher, null);
//
//                    launcher?.Dispose();
//
//                    activeProject = null;
//                }
//            }
//        }
//
//        /// <inheritdoc />
//        public event EventHandler StateChanged;
//
//        private void Instantiate()
//        {
//            try
//            {
//                lock (syncRoot)
//                {
//                    Project project = activeProject;
//
//                    ProjectLauncher old = Interlocked.Exchange(ref projectLauncher, null);
//
//                    old?.Dispose();
//
//                    if (project == null)
//                    {
//                        return;
//                    }
//
//                    ProjectCompileSettings settings = new ProjectCompileSettings();
//
//                    settings.OutputPath = project.ProjectFolder.CreateSubdirectory("bin")
//                        .FullName;
//                    settings.Version = project.Version++;
//                    settings.AssemblyName = $"{project.Uri}";
//                    settings.ProjectFolder = project.ProjectFolder.FullName;
//
//                    ProjectBuilder builder = null;
//
//                    try
//                    {
//                        builder = new ProjectBuilder(project, settings);
//
//                        ProjectCompileResult result = builder.Compile();
//
//                        project.Errors = result.Errors;
//
//                        if (builder.Compiled == false)
//                        {
//                            return;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        builder?.Build.Log("Uncaught exception in script init");
//                        builder?.Build.Log(ex.ToString());
//
//                        return;
//                    }
//
//                    ProjectLauncher launcher = null;
//
//                    try
//                    {
//                        launcher = new ProjectLauncher(project, settings);
//
//                        Interlocked.Exchange(ref projectLauncher, launcher);
//
//                        launcher.Launch(serverUri);
//                    }
//                    catch (Exception ex)
//                    {
//                        launcher?.Log.Log("Uncaught exception in script init");
//                        launcher?.Log.Log(ex.ToString());
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Unexpected exception in");
//                Console.WriteLine(ex.ToString());
//            }
//            finally
//            {
//                ProjectChanged?.Invoke(Project);
//            }
//        }
//
//        private void OnWatchFilesChanged(object sender, EventArgs e)
//        {
//            Updated();
//        }
//
//        private void Updated()
//        {
//            Interlocked.Increment(ref updated);
//
//            Task.Run(() => Instantiate());
//        }
//    }
}
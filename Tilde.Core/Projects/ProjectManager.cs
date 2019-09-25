// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tilde.Core.Controls;
using Tilde.SharedTypes;

namespace Tilde.Core.Projects
{
    public delegate void ProjectEvent(Project project);

    public delegate void ProjectsEvent(Uri[] projects);

    public delegate void FileEvent(Uri project, Uri file, string hash);

    public class ProjectManager : IDisposable
    {
        public readonly string ProjectRoot;

        public readonly ConcurrentDictionary<Uri, Project> Projects = new ConcurrentDictionary<Uri, Project>();
        private readonly EventDebouncer eventDebouncer;
        private readonly FileSystemWatcher watcher;
        private readonly DirectoryInfo projectRootDirectoryInfo; 

        public ProjectManager(string projectRoot)
        {
            ProjectRoot = projectRoot;

            projectRootDirectoryInfo = new DirectoryInfo(ProjectRoot);

            if (projectRootDirectoryInfo.Exists == false)
            {
                projectRootDirectoryInfo.Create();
            }

            eventDebouncer = new EventDebouncer(Scan, 100, 1000);

            watcher = new FileSystemWatcher
            {
                Path = projectRootDirectoryInfo.FullName,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            Cache();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            watcher?.Dispose();
            eventDebouncer?.Dispose();
        }

//        public async Task<bool> Archive(Uri project)
//        {
//            DirectoryInfo projectRoot = new DirectoryInfo(ProjectRoot);
//
//            FileInfo file = new FileInfo(
//                Path.Combine(
//                    projectRoot.FullName,
//                    $"{project}-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.zip"
//                )
//            );
//
//            byte[] bytes = await Projects[project]
//                .Pack();
//
//            using (Stream stream = File.Open(
//                file.FullName,
//                FileMode.Create,
//                FileAccess.Write,
//                FileShare.None
//            ))
//            {
//                await stream.WriteAsync(bytes, 0, bytes.Length);
//            }
//
//            return true;
//        }

        public event ControlPanelEvent ControlPanelChanged;
        public event ControlValueEvent ControlUpdated;
        public event ControlValueEvent ControlValueChanged;
        public event DataSourceEvent DataSourceChanged;

        public event FileEvent FileChanged;
        public event ProjectEvent ProjectChanged;
        public event ProjectsEvent ProjectsChanged;

        public Project Read(Uri project)
        {
            DirectoryInfo projectRoot = new DirectoryInfo(ProjectRoot);

            DirectoryInfo directory = new DirectoryInfo(Path.Combine(projectRoot.FullName, project.ToString()));

            Uri projectUri = new Uri(directory.Name, UriKind.RelativeOrAbsolute);

            Project result = Projects.GetOrAdd(projectUri, uri => CreateProject(directory));

            return result;
        }

//        public async Task<bool> Restore(Uri project)
//        {
//            DirectoryInfo projectRoot = new DirectoryInfo(ProjectRoot);
//
//            FileInfo file = new FileInfo(Path.Combine(projectRoot.FullName, project.ToString()));
//
//            if (file.Exists == false)
//            {
//                return false;
//            }
//
//            Uri projectUri = new Uri(Path.GetFileNameWithoutExtension(file.Name), UriKind.RelativeOrAbsolute);
//
//            DirectoryInfo directory = new DirectoryInfo(Path.Combine(projectRoot.FullName, projectUri.ToString()));
//
//            Project result = Projects.GetOrAdd(projectUri, uri => CreateProject(directory));
//
//            await result.Unpack(File.ReadAllBytes(file.FullName));
//
//            return true;
//        }

        private void Cache()
        {
            eventDebouncer.Invoke();
        }

        private Project CreateProject(DirectoryInfo directory)
        {
            Project project = new Project(directory);

            project.ProjectChanged += ProjectOnProjectChanged;
            project.FileChanged += ProjectOnFileChanged;

            project.Controls.ControlPanelChanged += OnControlPanelChanged;
            project.Controls.ControlValueChanged += OnControlValueChanged;
            project.Controls.ControlUpdated += OnControlUpdated;
            project.Controls.DataSourceChanged += OnDataSourceChanged;

            ProjectOnProjectChanged(project);

            return project;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (IsInContextOfProject(e, out Project project) == false)
            {
                Cache();
                
                return;
            }

            project?.OnAnyChanged(sender, e);
        }

        private bool IsInContextOfProject(FileSystemEventArgs e, out Project project)
        {
            project = null;

            string pathString = e.FullPath
                .Substring(projectRootDirectoryInfo.FullName.Length + 1)
                .Replace('\\', '/');

            string projectString = pathString
                .Substring(0, pathString.IndexOf('/') + 1)
                .TrimEnd('/');

            Uri projectUri = new Uri(projectString, UriKind.RelativeOrAbsolute);

            return Projects.TryGetValue(projectUri, out project) && 
                   pathString.Length > projectString.Length + 1;
        }

        private void OnControlPanelChanged(Uri project, Uri panelUri, ControlPanel panel)
        {
            ControlPanelChanged?.Invoke(project, panelUri, panel);
        }

        private void OnControlUpdated(
            Uri project,
            Uri control,
            string connectionId,
            object value)
        {
            ControlUpdated?.Invoke(project, control, connectionId, value);
        }

        private void OnControlValueChanged(
            Uri project,
            Uri control,
            string connectionId,
            object value)
        {
            ControlValueChanged?.Invoke(project, control, connectionId, value);
        }

        private void OnDataSourceChanged(Uri project, Uri uri, DataSource dataSource)
        {
            DataSourceChanged?.Invoke(project, uri, dataSource);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (IsInContextOfProject(e, out Project project) == false)
            {
                if (project == null) 
                {
                    return;
                }

                project.ProjectChanged -= ProjectOnProjectChanged;
                project.FileChanged -= ProjectOnFileChanged;
                project.Controls.ControlPanelChanged -= OnControlPanelChanged;
                project.Controls.ControlValueChanged -= OnControlValueChanged;
                project.Controls.ControlUpdated -= OnControlUpdated;
                project.Controls.DataSourceChanged -= OnDataSourceChanged;

                project.Dispose();
                project.Deleted = true;

                Cache();
                
                return;
            }

            project?.OnDeleted(sender, e);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (IsInContextOfProject(e, out Project project) == false)
            {
                Cache();
                
                return;
            }

            project?.OnRenamed(sender, e);
        }

        private void ProjectOnFileChanged(Uri project, Uri file, string hash)
        {
            FileChanged?.Invoke(project, file, hash);
        }

        private void ProjectOnProjectChanged(Project project)
        {
            ProjectChanged?.Invoke(project);
        }

        private void Scan()
        {
            DirectoryInfo projectRoot = new DirectoryInfo(ProjectRoot);

            if (projectRoot.Exists == false)
            {
                projectRoot.Create();
            }

            Debug.WriteLine("Scanning project root " + projectRoot.FullName);

            List<Uri> currentProjects = new List<Uri>(Projects.Keys);

            foreach (DirectoryInfo directory in projectRoot.GetDirectories())
            {
                Uri projectUri = new Uri(directory.Name, UriKind.RelativeOrAbsolute);

                Projects.GetOrAdd(projectUri, uri => CreateProject(directory));

                currentProjects.Remove(projectUri);
            }

            foreach (Uri projectUri in currentProjects)
            {
                if (Projects.TryRemove(projectUri, out Project project) == false)
                {
                    continue;
                }

                project.Dispose();
                project.Deleted = true;
            }

            ProjectsChanged?.Invoke(Projects.Keys.ToArray());
        }
    }
}
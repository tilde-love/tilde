// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Tilde.Core;
using Tilde.Core.Controls;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Host.Hubs.Client
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClientService : IHostedService
    {
        private readonly IHubContext<ClientHub, IClient> hubContext;
        private readonly ProjectManager projectManager;
        private readonly IRuntime runtime;

        public ClientService(IHubContext<ClientHub, IClient> hubContext, IRuntime runtime, ProjectManager projectManager)
        {
            this.hubContext = hubContext;
            this.runtime = runtime;
            this.projectManager = projectManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            runtime.StateChanged += StateChanged;
            runtime.ProjectChanged += ProjectChanged;

            projectManager.ProjectsChanged += ProjectsChanged;
            projectManager.ProjectChanged += ProjectChanged;
            projectManager.FileChanged += FileChanged;

            projectManager.ControlPanelChanged += ControlPanelChanged;
            projectManager.DataSourceChanged += DataSourceChanged;
            projectManager.ControlValueChanged += ControlValueChanged;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            runtime.StateChanged -= StateChanged;
            runtime.ProjectChanged -= ProjectChanged;

            projectManager.ProjectsChanged -= ProjectsChanged;
            projectManager.ProjectChanged -= ProjectChanged;
            projectManager.FileChanged -= FileChanged;

            projectManager.ControlPanelChanged -= ControlPanelChanged;
            projectManager.DataSourceChanged -= DataSourceChanged;
            projectManager.ControlValueChanged -= ControlValueChanged;

            return Task.CompletedTask;
        }

        private async void ControlPanelChanged(Uri project, Uri panelUri, ControlPanel panel)
        {
            await hubContext
                .Clients
                .All
                .OnControlPanel(project, panelUri, panel);
        }

        private async void ControlValueChanged(Uri project, Uri control, string connectionId, object value)
        {
            if (connectionId != null)
            {
                await hubContext
                    .Clients
                    .AllExcept(connectionId)
                    .OnControlValue(project, control, value);
            }
            else
            {
                await hubContext
                    .Clients
                    .All
                    .OnControlValue(project, control, value);
            }
        }

        private async void DataSourceChanged(Uri project, Uri uri, DataSource dataSource)
        {
            await hubContext
                .Clients
                .All
                .OnDataSource(project, uri, dataSource);
        }

        private async void FileChanged(Uri project, Uri file, string hash)
        {
            await hubContext
                .Clients
                .All
                .OnFile(project, file, hash);
        }

        private async void ProjectChanged(Project project)
        {
            await hubContext
                .Clients
                .All
                .OnProject(project);
        }

        private async void ProjectsChanged(Uri[] projects)
        {
            await hubContext
                .Clients
                .All
                .OnProjects(projects);
        }

        private async void StateChanged(object sender, EventArgs e)
        {
            await hubContext
                .Clients
                .All
                .OnStateChange(runtime.State);
        }
    }
}
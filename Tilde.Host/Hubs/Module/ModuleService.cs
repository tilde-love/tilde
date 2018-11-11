// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Tilde.Core;
using Tilde.Core.Projects;

namespace Tilde.Host.Hubs.Module
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModuleService : IHostedService
    {
        private readonly IHubContext<ModuleHub, IModuleClient> hubContext;
        private readonly ProjectManager projectManager;
        private readonly IRuntime runtime;

        public ModuleService(IHubContext<ModuleHub, IModuleClient> hubContext, IRuntime runtime, ProjectManager projectManager)
        {
            this.hubContext = hubContext;
            this.runtime = runtime;
            this.projectManager = projectManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            projectManager.ControlUpdated += ControlUpdated;
            
            return Task.CompletedTask;
        }

        private async void ControlUpdated(Uri project, Uri control, string connectionId, object value)
        {
            await hubContext
                .Clients
                .Group(project.ToString())
                .OnValueChanged(control, connectionId, value); 
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            projectManager.ControlUpdated -= ControlUpdated;
            
            return Task.CompletedTask;
        }
    }
}
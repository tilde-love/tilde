// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Tilde.Core;
using Tilde.Core.Projects;

namespace Tilde.Host
{
    public interface INotifyClient
    {
//        Task OnErrorMessage(string message);
//
//        Task OnIsInError(bool isInError);

        Task OnScriptProject(Project project);

        Task OnStateChange(RuntimeState state);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class NotifyService : IHostedService
    {
        private readonly IHubContext<NotifyHub, INotifyClient> hubContext;
        private readonly ProjectManager projectManager;
        private readonly IRuntime runtime;

        public NotifyService(IHubContext<NotifyHub, INotifyClient> hubContext, IRuntime runtime, ProjectManager projectManager)
        {
            this.hubContext = hubContext;
            this.runtime = runtime;
            this.projectManager = projectManager;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            runtime.StateChanged += RuntimeOnStateChanged;
//            runtime.ErrorMessageChanged += ScriptHostOnErrorMessageChanged;
            runtime.ProjectChanged += ScriptHostOnScriptProjectChanged;
//            runtime.IsInErrorChanged += ScriptHostOnIsInErrorChanged;
            projectManager.ProjectEvent += ProjectManagerOnProjectEvent;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            runtime.StateChanged -= RuntimeOnStateChanged;
//            runtime.ErrorMessageChanged -= ScriptHostOnErrorMessageChanged;
            runtime.ProjectChanged -= ScriptHostOnScriptProjectChanged;
//            runtime.IsInErrorChanged -= ScriptHostOnIsInErrorChanged;

            projectManager.ProjectEvent -= ProjectManagerOnProjectEvent;

            return Task.CompletedTask;
        }

        private void ProjectManagerOnProjectEvent(Project project)
        {
            hubContext.Clients.All.OnScriptProject(project)
                .Wait();
        }
//
//        private void ScriptHostOnErrorMessageChanged(object sender, EventArgs e)
//        {
//            hubContext.Clients.All.OnErrorMessage(runtime.ErrorMessage)
//                .Wait();
//        }
//
//        private void ScriptHostOnIsInErrorChanged(object sender, EventArgs e)
//        {
//            hubContext.Clients.All.OnIsInError(runtime.IsInError)
//                .Wait();
//        }

        private void ScriptHostOnScriptProjectChanged(object sender, EventArgs e)
        {
            hubContext.Clients.All.OnScriptProject(runtime.Project)
                .Wait();
        }

        private void RuntimeOnStateChanged(object sender, EventArgs e)
        {
            hubContext.Clients.All.OnStateChange(runtime.State)
                .Wait();
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class NotifyHub : Hub<INotifyClient>
    {
        /// <inheritdoc />
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Notify client connected ({Context.ConnectionId})");

            return base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Notify client disconnected ({Context.ConnectionId})");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
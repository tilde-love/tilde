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
    public interface IModuleClient
    {
//        Task OnErrorMessage(string message);
//
//        Task OnIsInError(bool isInError);
//
//        Task OnScriptProject(Project project);
//
//        Task OnStateChange(ScriptHostState state);
    }

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


//
//        /// <inheritdoc />
//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            runtime.StateChanged += ScriptHostOnStateChanged;
//            runtime.ErrorMessageChanged += ScriptHostOnErrorMessageChanged;
//            runtime.ScriptProjectChanged += ScriptHostOnScriptProjectChanged;
//            runtime.IsInErrorChanged += ScriptHostOnIsInErrorChanged;
//            projectManager.ProjectEvent += ProjectManagerOnProjectEvent;
//
//            return Task.CompletedTask;
//        }
//
//        /// <inheritdoc />
//        public Task StopAsync(CancellationToken cancellationToken)
//        {
//            runtime.StateChanged -= ScriptHostOnStateChanged;
//            runtime.ErrorMessageChanged -= ScriptHostOnErrorMessageChanged;
//            runtime.ScriptProjectChanged -= ScriptHostOnScriptProjectChanged;
//            runtime.IsInErrorChanged -= ScriptHostOnIsInErrorChanged;
//
//            projectManager.ProjectEvent -= ProjectManagerOnProjectEvent;
//
//            return Task.CompletedTask;
//        }
//
//        private void ProjectManagerOnProjectEvent(Project project)
//        {
//            hubContext.Clients.All.OnScriptProject(project)
//                .Wait();
//        }
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
//
//        private void ScriptHostOnScriptProjectChanged(object sender, EventArgs e)
//        {
//            hubContext.Clients.All.OnScriptProject(runtime.ActiveProject)
//                .Wait();
//        }
//
//        private void ScriptHostOnStateChanged(object sender, EventArgs e)
//        {
//            hubContext.Clients.All.OnStateChange(runtime.State)
//                .Wait();
//        }
        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModuleHub : Hub<IModuleClient>
    {
        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Module client connected ({Context.ConnectionId})");

            string moduleName = Context.GetHttpContext()
                .Request.Query["m"];

            Console.WriteLine("Module: " + moduleName);

            await Groups.AddToGroupAsync(Context.ConnectionId, moduleName);

            await base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Module client disconnected ({Context.ConnectionId})");

            string moduleName = Context.GetHttpContext()
                .Request.Query["m"];

            Console.WriteLine("Module: " + moduleName);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, moduleName);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine("SendMessage!! " + user);
            Console.WriteLine(message);

            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
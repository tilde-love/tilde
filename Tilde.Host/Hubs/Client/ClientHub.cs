// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Tilde.Core.Projects;

namespace Tilde.Host.Hubs.Client
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ClientHub : Hub<IClient>
    {
        private readonly ProjectManager projectManager;

        public ClientHub(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        /// <inheritdoc />
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected ({Context.ConnectionId})");

            return base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected ({Context.ConnectionId})");

            return base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateValue(Uri project, Uri uri, object value)
        {
            Console.WriteLine($"UpdateValue: {project}/{uri} {value}");

            if (projectManager.Projects.TryGetValue(project, out Project proj) == false)
            {
                return;
            }

            //await proj.Controls.UpdateValue(uri, Context.ConnectionId, value); 
            await proj.Controls.UpdateValue(uri, null, value);
        }
    }
}
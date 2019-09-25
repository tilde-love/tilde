// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Host.Hubs.Module
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModuleHub : Hub<IModuleClient>
    {
        private readonly ProjectManager projectManager;

        public ModuleHub(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
        }

        public async Task DefineDataSource(
            Uri project,
            Uri uri,
            DataSourceType type,
            bool @readonly,
            NumericRange? range,
            string[] values,
            Graph graph)
        {
            Console.WriteLine($"DefineDataSource: {project}/{uri} {type} {(@readonly ? "readonly" : "")}");

            if (projectManager.Projects.TryGetValue(project, out Project proj) == false)
            {
                return;
            }

            await proj.Controls.DefineDataSource(uri, type, @readonly, range, values, graph);
        }

        public async Task DeleteDataSource(Uri project, Uri uri)
        {
            Console.WriteLine($"DeleteDataSource: {project}/{uri}");

            if (projectManager.Projects.TryGetValue(project, out Project proj) == false)
            {
                return;
            }

            await proj.Controls.DeleteDataSource(uri);
        }

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

        public async Task SetValue(
            Uri project,
            Uri uri,
            string connectionId,
            object value)
        {
//            Console.WriteLine($"SetValue: {project}/{uri} ({(connectionId ?? "NULL")})");
//            Console.WriteLine(JsonConvert.SerializeObject(value, Formatting.None));

            if (projectManager.Projects.TryGetValue(project, out Project proj) == false)
            {
                return;
            }

            await proj.Controls.SetValue(uri, connectionId, value);
        }

//        public async Task SendMessage(string user, string message)
//        {
//            Console.WriteLine("SendMessage!! " + user);
//            Console.WriteLine(message);
//
//            //await Clients.All.SendAsync("ReceiveMessage", user, message);
//        }
    }
}
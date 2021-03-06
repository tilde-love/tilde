// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Tilde.SharedTypes;

namespace Tilde.Module
{
    public delegate void ValueUpdated(Uri uri, string connectionId, object value);

    public class ModuleConnection
    {
        private readonly CancellationToken cancellationToken;
        private readonly HubConnection connection;
        private readonly IDisposable controlUpdated;
        private bool disposing;
        private readonly Uri moduleName;
        private readonly Uri uri;
        private readonly ConcurrentDictionary<Uri, ValueUpdated> valueHandlers = new ConcurrentDictionary<Uri, ValueUpdated>();

        public ModuleConnection(Uri uri, Uri moduleName, CancellationToken cancellationToken)
        {
            this.uri = uri;
            this.moduleName = moduleName;
            this.cancellationToken = cancellationToken;

            connection = new HubConnectionBuilder()
                .WithUrl($"{uri}?m={moduleName}", HttpTransportType.WebSockets)
                .Build();

            connection.Closed += ConnectionOnClosed;

            controlUpdated = connection.On<Uri, string, object>("OnValueChanged", OnValueChanged);
        }

        public async Task DefineDataSource(
            Uri uri,
            DataSourceType type,
            bool @readonly,
            NumericRange? range,
            string[] values,
            Graph graph)
        {
            await connection.InvokeAsync(
                "DefineDataSource",
                moduleName,
                uri,
                type,
                @readonly,
                range,
                values,
                graph,
                cancellationToken
            );
        }

        public async Task DeleteDataSource(Uri uri)
        {
            await connection.InvokeAsync("DeleteDataSource", moduleName, uri, CancellationToken.None);
        }

        public async Task DisposeAsync()
        {
            disposing = true;

            controlUpdated.Dispose();

            await connection.DisposeAsync();
        }

        public void RegisterValueChange(Uri uri, ValueUpdated callback)
        {
            if (valueHandlers.TryAdd(uri, callback) == false)
            {
                throw new Exception($"Value handler for {uri} has already been registered.");
            }
        }

        public async Task SetValue(Uri uri, string connectionId, object value)
        {
            await connection.InvokeAsync("SetValue", moduleName, uri, connectionId, value, cancellationToken);
        }

        public async Task StartAsync()
        {
            await connection.StartAsync(cancellationToken);

            Console.WriteLine($"Started connection to {uri}");
        }

        public async Task StopAsync()
        {
            await connection.StopAsync(cancellationToken);

            Console.WriteLine($"Stopped connection to {uri}");
        }

        public void UnregisterValueChange(Uri uri)
        {
            valueHandlers.TryRemove(uri, out ValueUpdated _);
        }

        private async Task ConnectionOnClosed(Exception ex)
        {
            Console.WriteLine("Connection closed");

            if (ex != null)
            {
                Console.WriteLine(ex.ToString());
            }

            if (disposing)
            {
                return;
            }

            await connection.StartAsync(cancellationToken);
        }

        private void OnValueChanged(Uri uri, string connectionId, object value)
        {
            Console.WriteLine($"OnValueChanged: {uri}, {connectionId}, {value}");

            if (valueHandlers.TryGetValue(uri, out ValueUpdated updated) == false)
            {
                return;
            }

            updated(uri, connectionId, value);
        }
    }
}
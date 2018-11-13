// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Tilde.Core;
using Tilde.Core.Controls;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Host.Hubs.Client
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public interface IClient
    {
        Task OnControlPanel(Uri project, Uri panelUri, ControlPanel panel);
        
        Task OnControlValue(Uri project, Uri control, object value);

        Task OnDataSource(Uri project, Uri uri, DataSource dataSource);

        Task OnFile(Uri project, Uri file, string hash);

        Task OnProject(Project project);

        Task OnProjects(Uri[] projects);

        Task OnStateChange(RuntimeState state);
    }
}
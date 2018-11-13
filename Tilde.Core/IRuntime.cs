// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Tilde.Core.Projects;

namespace Tilde.Core
{
    public interface IRuntime : IDisposable
    {
        Project Project { get; }

        RuntimeState State { get; set; }
        
        Uri ServerUri { get; set; }

        void Load(Project project);

        event ProjectEvent ProjectChanged;

        event EventHandler StateChanged;
    }
}
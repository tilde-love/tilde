// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Module
{
    public interface IRunnable
    {
        void Pause();

        void Play();

        Task Run(ModuleConnection connection, CancellationToken cancellationToken);
    }
}
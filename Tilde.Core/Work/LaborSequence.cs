// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Core.Work
{
    public class LaborSequence : ILaborRunner
    {
        public List<ILaborRunner> Steps { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            
        }

        /// <inheritdoc />
        public string Type { get; }

        /// <inheritdoc />
        public LaborState State { get; set; }

        /// <inheritdoc />
        public async Task<RunResult> Work(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
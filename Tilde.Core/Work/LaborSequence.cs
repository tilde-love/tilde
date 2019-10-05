// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tilde.Core.Projects;

namespace Tilde.Core.Work
{
    public class LaborSequence : ILaborRunner
    {
        public List<ILaborRunner> Steps { get; set; }

        /// <inheritdoc />
        public string Type { get; }

        /// <inheritdoc />
        public LaborState State { get; set; }

        /// <inheritdoc />
        public event Action<ILaborRunner, LaborState> StateChanged;

        /// <inheritdoc />
        public async Task<(int? exitCode, string message)> Work(Project project, CancellationToken cancellationToken)
        {
            foreach (ILaborRunner step in Steps)
            {
                (int? exitCode, string message) result = await step.Work(project, cancellationToken);

                if (result.exitCode != 0)
                {
                    return result; 
                }
            }

            return (0, "Completed");
        }
    }
}
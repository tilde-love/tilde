// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Core.Work
{
    /// <summary>
    /// Proxy class for StreamWriter. Find a better way to to do this. 
    /// </summary>
    internal class AsyncQueue
    {
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private ManualResetEventSlim hasMessage = new ManualResetEventSlim(); 

        public void Enqueue(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            queue.Enqueue(value);
            
            hasMessage.Set();
        }

        public async Task<string> DequeueAsync(CancellationToken cancellationToken)
        {
            while (await hasMessage.WaitHandle.WaitOneAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null; 
                }

                if (queue.TryDequeue(out string value) == false)
                {
                    continue;
                }

                hasMessage.Reset();
                    
                return value;
            }

            return null; 
        }
    }
}
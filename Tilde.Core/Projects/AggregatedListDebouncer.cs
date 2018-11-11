// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Core.Projects
{
    public class AggregatedListDebouncer<T> : IDisposable
    {
        private int callState;

        private readonly Action<T[]> function;
        private readonly long interval;
        private readonly int intervalMs;
        private Task invokeTask;
        private long lastCall;
        private Dictionary<T, int> list = new Dictionary<T, int>();
        private readonly long offset;

        public AggregatedListDebouncer(Action<T[]> function, int offset, int interval)
        {
            this.offset = TimeSpan.FromMilliseconds(offset).Ticks;
            intervalMs = interval;
            this.interval = TimeSpan.FromMilliseconds(interval).Ticks;
            this.function = function;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            invokeTask?.Wait();
        }

        public void Invoke(T item)
        {
            if (Interlocked.Increment(ref callState) > 1)
            {
                Interlocked.Exchange(ref lastCall, DateTime.UtcNow.Ticks);

                Interlocked.Decrement(ref callState);

                list[item] = 1;

                return;
            }

            Interlocked.Exchange(ref lastCall, DateTime.UtcNow.Ticks + offset);

            invokeTask?.Wait();

            invokeTask = Task.Run(
                () =>
                {
                    try
                    {
                        while (lastCall + interval > DateTime.UtcNow.Ticks)
                        {
                            Task.Delay(intervalMs)
                                .Wait();
                        }

                        Dictionary<T, int> final = Interlocked.Exchange(ref list, new Dictionary<T, int>());

                        function(final.Keys.ToArray());
                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine(ex.Message);
//                    }
                    finally
                    {
                        Interlocked.Decrement(ref callState);
                    }
                }
            );
        }
    }
}
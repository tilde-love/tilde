// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Core.Projects
{
    public class EventDebouncer : IDisposable
    {
        private int callState;

        private readonly Action function;
        private readonly long interval;
        private readonly int intervalMs;
        private Task invokeTask;
        private long lastCall;
        private readonly long offset;

        public EventDebouncer(Action function, int offset, int interval)
        {
            this.offset = TimeSpan.FromMilliseconds(offset)
                .Ticks;
            intervalMs = interval;
            this.interval = TimeSpan.FromMilliseconds(interval)
                .Ticks;
            this.function = function;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            invokeTask?.Wait();
        }

        public void Invoke()
        {
            if (Interlocked.Increment(ref callState) > 1)
            {
                Interlocked.Exchange(ref lastCall, DateTime.UtcNow.Ticks);

                Interlocked.Decrement(ref callState);

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

                        function();
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
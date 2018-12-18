﻿// Copyright (c) Interactive Scientific LTD. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tilde.Module
{
    /// <summary>
    ///     A mechanism for tracking elapsed time.
    /// </summary>
    public sealed class Clock
    {
        private readonly long frequency;
        private readonly double frequencyMs;
        private bool isRunning;
        private long lastUpdate;
        private double minimumDelayTime;
        private readonly float minSpinThreshold = 0.2f;
        private readonly double oneMillisecond;

        public bool AdaptiveWait { get; set; } = true;
        public double AdaptiveWaitOffset { get; private set; } = 10;
        public double MinimumDelayTime { get; private set; }
        public int TaskDelayMiliseconds { get; private set; }
        public float TimeSpentInDelay { get; private set; }
        public float TimeSpentSpinning { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Clock" /> class.
        /// </summary>
        /// <autogeneratedoc />
        public Clock()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                case PlatformID.Xbox:
                    minSpinThreshold = 4f;
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    minSpinThreshold = 0.1f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            frequency = Stopwatch.Frequency;
            frequencyMs = frequency / 1000d;

            oneMillisecond = 1 * frequencyMs;
        }

        public static string GetTimeString(double seconds)
        {
            double time = seconds;

            if (time > 1)
            {
                return $"{(int) time}s";
            }

            time *= 1000;

            if (time > 1)
            {
                return $"{(int) time}ms";
            }

            time *= 1000;

            return $"{(int) time}μs";
        }

        /// <summary>
        ///     Spins the till.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <autogeneratedoc />
        public float SpinTill(float value)
        {
            long spinStart = Stopwatch.GetTimestamp();
            long currentTime = spinStart;

            float accumulator = 0;

            if (isRunning == false)
            {
                currentTime = Stopwatch.GetTimestamp();

                TimeSpentSpinning = (float) ((currentTime - spinStart) / frequencyMs);

                return TimeSpentSpinning;
            }

            long count = lastUpdate;

            while (accumulator < value)
            {
                long last = count;
                count = Stopwatch.GetTimestamp();
                accumulator += (float) (count - last) / frequency;
            }

            currentTime = Stopwatch.GetTimestamp();

            TimeSpentSpinning = (float) ((currentTime - spinStart) / frequencyMs);

            return TimeSpentSpinning;
        }

        public async Task<float> SpinTillAsync(float value, CancellationToken cancel)
        {
            minimumDelayTime = Math.Max(0, minimumDelayTime - 0.1);

            if (isRunning == false)
            {
                return 0;
            }

            long startTime = lastUpdate;
            long absFrameEnd = startTime + (long) (value * frequency);

            // Get the total time between now and the last update
            long currentTime = Stopwatch.GetTimestamp();

            long maxDelayTicks = absFrameEnd - currentTime;

            int maxDelayMs = (int) (maxDelayTicks / frequencyMs);

            // calculate the adaptive delay 
            int valueDelayLimit = maxDelayMs - (AdaptiveWait ? (int) AdaptiveWaitOffset : 10);

            bool ranDelay = false;

            long delayStart = currentTime;

            MinimumDelayTime = minimumDelayTime;

            if (valueDelayLimit >= minimumDelayTime)
            {
                TaskDelayMiliseconds = valueDelayLimit;

                await Task.Delay(valueDelayLimit, cancel);

                currentTime = Stopwatch.GetTimestamp();

                if (currentTime > absFrameEnd)
                {
                    minimumDelayTime = Math.Min(16, minimumDelayTime + 1);
                }

                ranDelay = true;
            }

            TimeSpentInDelay = (float) ((currentTime - delayStart) / frequencyMs);

            long spinStart = currentTime;

            while (currentTime < absFrameEnd)
            {
                currentTime = Stopwatch.GetTimestamp();
            }

            TimeSpentSpinning = (float) ((currentTime - spinStart) / frequencyMs);

            if (!ranDelay || !AdaptiveWait)
            {
                return TimeSpentSpinning;
            }

            if (TimeSpentSpinning < 2)
            {
                AdaptiveWaitOffset += 0.1;
            }
            else if (TimeSpentSpinning > minSpinThreshold)
            {
                AdaptiveWaitOffset -= 0.1;
            }

            AdaptiveWaitOffset = Math.Min(value * 1000 - 2, Math.Max(0, AdaptiveWaitOffset));

            return TimeSpentSpinning;
        }

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        /// <autogeneratedoc />
        public void Start()
        {
            lastUpdate = Stopwatch.GetTimestamp();
            isRunning = true;
        }

        /// <summary>
        ///     Time since update
        /// </summary>
        /// <returns>The time, in seconds, that elapsed since the previous update.</returns>
        public float TimeSinceUpdate()
        {
            float result = 0.0f;

            if (isRunning)
            {
                result = (float) (Stopwatch.GetTimestamp() - lastUpdate) / frequency;
            }

            return result;
        }

        /// <summary>
        ///     Updates the clock.
        /// </summary>
        /// <returns>The time, in seconds, that elapsed since the previous update.</returns>
        public double Update()
        {
            double result = 0.0;

            if (!isRunning)
            {
                return result;
            }

            long last = lastUpdate;

            lastUpdate = Stopwatch.GetTimestamp();

            result = (double) (lastUpdate - last) / frequency;

            return result;
        }
    }
}
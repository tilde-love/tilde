// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Tilde.Core.Work
{
    public enum LaborState
    {
        Running = 0,
        Stopped,
        Paused,
        ExitedWithCode, 
        Terminating,
        Restarting,
        ExitedWithoutCode,
        Exited
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tilde.SharedTypes
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModuleCommand
    {
        Play,
        Pause,
        Exit
    }
}
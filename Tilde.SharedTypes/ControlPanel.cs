// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tilde.SharedTypes
{
    public class ControlPanel
    {
        [JsonProperty("controls")]
        public List<Control> Controls { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
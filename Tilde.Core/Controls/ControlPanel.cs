// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace Tilde.Core.Controls
{
    public class ControlPanel
    {
        [JsonProperty("controls")]
        public List<Control> Controls { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; } 
    }
}
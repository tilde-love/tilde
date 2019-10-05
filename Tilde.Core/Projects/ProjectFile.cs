// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Tilde.Core.Projects
{
    public struct ProjectFile
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }
    }
}
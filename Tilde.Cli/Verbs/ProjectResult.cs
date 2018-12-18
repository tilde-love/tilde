// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tilde.Core.Controls;

namespace Tilde.Cli.Verbs
{
    class ProjectResult
    {
        [JsonProperty("controls")] public ControlGroup Controls;
        [JsonProperty("files")] public List<Uri> Files;
        [JsonProperty("uri")] public Uri Uri;
    }
}
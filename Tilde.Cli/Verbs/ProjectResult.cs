// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tilde.Core.Controls;
using Tilde.Core.Projects;

namespace Tilde.Cli.Verbs
{
    internal class ProjectResult
    {
        [JsonProperty("controls")] public ControlGroup Controls;
        [JsonProperty("files")] public List<ProjectFile> Files;
        [JsonProperty("uri")] public Uri Uri;
    }
}
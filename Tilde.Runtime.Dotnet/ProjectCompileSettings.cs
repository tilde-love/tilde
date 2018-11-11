// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Tilde.Runtime.Dotnet
{
    public class ProjectCompileSettings
    {
        public string AssemblyName { get; set; }

        public string OutputPath { get; set; }

        public string ProjectFolder { get; set; }

        public Dictionary<string, string> Sources { get; set; }

        public int Version { get; set; }
    }
}
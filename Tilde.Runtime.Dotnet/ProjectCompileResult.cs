// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Tilde.Core.Projects;

namespace Tilde.Runtime.Dotnet
{
    public class ProjectCompileResult
    {
        public string AssemblyPath { get; set; }

        public string SymbolsPath { get; set; }

        public Dictionary<Uri, List<Error>> Errors { get; set; }

    }
}
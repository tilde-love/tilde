// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;
using Tilde.Core.Projects;

namespace Tilde.Core.Build
{
    public struct Error : IComparable<Error>
    {
        [JsonProperty("type")] public string Type;

        [JsonProperty("text")] public string Text;

        [JsonProperty("span")] public ErrorSpan Span;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type.ToUpperInvariant()} ({Span}): {Text}";
        }

        /// <inheritdoc />
        public int CompareTo(Error other)
        {
            return Span.CompareTo(other.Span);
        }
    }
}
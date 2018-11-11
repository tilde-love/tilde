// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Tilde.Core.Projects
{
    public struct ErrorSpan : IComparable<ErrorSpan>
    {
        [JsonProperty("sl")] public int StartLine;

        [JsonProperty("sc")] public int StartColumn;

        [JsonProperty("el")] public int EndLine;

        [JsonProperty("ec")] public int EndColumn;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{StartLine}, {StartColumn}, {EndLine}, {EndColumn}";
        }

        /// <inheritdoc />
        public int CompareTo(ErrorSpan other)
        {
            int startLineComparison = StartLine.CompareTo(other.StartLine);

            if (startLineComparison != 0)
            {
                return startLineComparison;
            }

            return StartColumn.CompareTo(other.StartColumn);
        }
    }
}
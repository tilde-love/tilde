// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Tilde.SharedTypes
{
    public class DataSource
    {
        [JsonProperty("graph")] public Graph Graph;

        /// <summary>
        ///     Numerical value range
        /// </summary>
        [JsonProperty("range")]
        public NumericRange? NumericRange;

        [JsonProperty("values")] public string[] Values;

        [JsonProperty("type")]
        public DataSourceType DataSourceType { get; set; }

        [JsonProperty("readonly")]
        public bool Readonly { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Uri} ({(Readonly ? "readonly " : "")}{DataSourceType})";
        }
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Tilde.Core.Controls
{
    public class DataSource
    {
        [JsonProperty("uri")]
        public Uri Uri { get; set; } 

        [JsonProperty("type")]
        public DataSourceType DataSourceType { get; set; } 

        [JsonProperty("readonly")]
        public bool Readonly { get; set; } 
        
        /// <summary>
        /// Numerical value range
        /// </summary>
        [JsonProperty("range")]
        public NumericRange? NumericRange;
        
        [JsonProperty("values")]
        public string[] Values;
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Tilde.SharedTypes
{
    /// <summary>
    /// Definition of a control
    /// </summary>
    public struct Control
    {
        /// <summary>
        /// Uri of the data source
        /// </summary>
        [JsonProperty("source")]
        public Uri DataSource;

        /// <summary>
        /// Control type
        /// </summary>
        [JsonProperty("type")]
        public ControlType ControlType;

        /// <summary>
        /// Layout
        /// </summary>
        [JsonProperty("size")]
        public LayoutSize Size;

        /// <summary>
        /// Label
        /// </summary>
        [JsonProperty("label")]
        public string Label;

        /// <summary>
        /// Numerical value range
        /// </summary>
        [JsonProperty("range")]
        public NumericRange? NumericRange;             
    }
}
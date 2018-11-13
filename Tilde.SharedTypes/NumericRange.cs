// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Newtonsoft.Json;

namespace Tilde.SharedTypes
{
    public struct NumericRange
    {
        [JsonProperty("min")]
        public double Minimum;
        
        [JsonProperty("max")]
        public double Maximum;
        
        [JsonProperty("step")]
        public double Step;
    }
}
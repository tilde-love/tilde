// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tilde.Core.Projects
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommunicationMethod
    {
        [EnumMember(Value = "none")] 
        None, 
        
        [EnumMember(Value = "tty")] 
        Tty, 
        
        [EnumMember(Value = "signalr")]
        SignalR,
    }

    public class ProjectDefinition
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("build")]
        public string BuildCommand { get; set; }
        
        [JsonProperty("exe")]
        public string Executable { get; set; }

        [JsonProperty("args")]
        public string Arguments { get; set; }

        [JsonProperty("output")]
        public string OutputDirectory { get; set; }

        [JsonProperty("working")]
        public string WorkingDirectory { get; set; }

        [JsonProperty("communication")]
        public CommunicationMethod CommunicationMethod { get; set; }
    }
}
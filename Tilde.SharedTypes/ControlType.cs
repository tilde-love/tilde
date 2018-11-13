// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Tilde.SharedTypes
{
    /// <summary>
    /// Control types
    /// </summary>
    // [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlType
    {
        /// <summary>
        /// Toggle 
        /// </summary>
        Toggle,   
        
        /// <summary>
        /// Checkbox or checkbox group
        /// </summary>
        Checkbox, 
        
        /// <summary>
        /// Radio button group 
        /// </summary>
        RadioButton, 
        
        /// <summary>
        /// Drop down select box
        /// </summary>
        SelectBox, 
        
        /// <summary>
        /// Chips selector 
        /// </summary>
        Chips,
        
        /// <summary>
        /// Text box
        /// </summary>
        TextBox,
        
        /// <summary>
        /// Value as a string
        /// </summary>
        Value, 
        
        /// <summary>
        /// Value slider
        /// </summary>
        Slider, 
        
        Graph,
        
        Svg,
        
        Color, 
        
        Image, 
        
        Markdown, 
        
        Break, 
    }
}
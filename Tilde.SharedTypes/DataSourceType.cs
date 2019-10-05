// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Tilde.SharedTypes
{
    /// <summary>
    ///     Data types for data sources.
    /// </summary>
    // [JsonConverter(typeof(StringEnumConverter))]
    public enum DataSourceType
    {
        /// <summary>
        ///     Boolean
        /// </summary>
        Boolean,

        /// <summary>
        ///     Enumeration
        /// </summary>
        Enum,

        /// <summary>
        ///     String
        /// </summary>
        String,

        /// <summary>
        ///     Floating point number
        /// </summary>
        Float,

        /// <summary>
        ///     Array of floating point numbers
        /// </summary>
        FloatArray,

        /// <summary>
        ///     Integer
        /// </summary>
        Integer,

        /// <summary>
        ///     Array of integers
        /// </summary>
        IntegerArray,

        /// <summary>
        ///     Color
        /// </summary>
        Color,

        /// <summary>
        ///     Image
        /// </summary>
        Image,

        /// <summary>
        ///     Graph object composed of graph lines and styles
        /// </summary>
        Graph,

        /// <summary>
        ///     Base64 encoded svg image
        /// </summary>
        Svg,

        /// <summary>
        ///     Unknown type
        /// </summary>
        Any
    }
}
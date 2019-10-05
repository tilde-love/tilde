// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Tilde.Core.Templates
{
    [TypeConverter(typeof(PackageNameTypeConverter))]
//    [JsonConverter(typeof(PackageNameJsonConverter))]
    public struct PackageName : IComparable<PackageName>, IEquatable<PackageName>
    {
        [JsonIgnore]
        private static readonly Regex parseRegex = new Regex(
            @"(?<name>^.+?)(-(?<version>\d+(.\d+)*))?$",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled
        );

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("v")]
        public Version Version { get; set; }

        public static bool TryParse(string name, out PackageName packageName)
        {
            packageName = default;

            Match match = parseRegex.Match(name);

            if (match.Success == false)
            {
                return false;
            }

            string nameString = match.Groups["name"].Value;

            Version version = null; 
            
            if (match.Groups["version"].Success == true && 
                Version.TryParse(match.Groups["version"].Value, out version) == false)
            {
                return false;
            }

            packageName = new PackageName {Name = nameString, Version = version};

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Version == null ? Name : $"{Name}-{Version}";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        /// <inheritdoc />
        public bool Equals(PackageName other)
        {
            return string.Equals(Name, other.Name) && Equals(Version, other.Version);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is PackageName other && Equals(other);
        }

        /// <inheritdoc />
        public int CompareTo(PackageName other)
        {
            int nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            
            if (nameComparison != 0)
            {
                return nameComparison;
            }

            return Comparer<Version>.Default.Compare(Version, other.Version);
        }
    }
//
//    public class PackageNameJsonConverter : JsonConverter<PackageName>
//    {
//        /// <inheritdoc />
//        public override void WriteJson(JsonWriter writer, PackageName value, JsonSerializer serializer)
//        {
//            writer.
//        }
//
//        /// <inheritdoc />
//        public override PackageName ReadJson(
//            JsonReader reader,
//            Type objectType,
//            PackageName existingValue,
//            bool hasExistingValue,
//            JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }

    public class PackageNameTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            
            return base.CanConvertFrom(context, sourceType);
        }
        
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string && PackageName.TryParse(value.ToString(), out PackageName packageName) == true)
            {
                return packageName; 
            }
            
            return base.ConvertFrom(context, culture, value);
        }
    }
}
// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using Tilde.Core.Projects;

namespace Tilde.Core
{
//    public interface IRuntime : IDisposable
//    {
//        Project Project { get; }
//
//        RuntimeState State { get; set; }
//
//        void Load(Project project);
//
//        event ProjectEvent ProjectChanged;
//
//        event EventHandler StateChanged;
//    }
//    
//    [JsonConverter(typeof(JsonSubtypes), "is_album")]
//    public class GalleryItemConverter : JsonConverter
//    {
//        public override bool CanConvert(Type objectType)
//        {
//            return typeof(GalleryItem).IsAssignableFrom(objectType);
//        }
//
//        public override object ReadJson(JsonReader reader, 
//            Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            JObject item = JObject.Load(reader);
//            if (item["is_album"].Value<bool>())
//            {
//                return item.ToObject<GalleryAlbum>();
//            }
//            else
//            {
//                return item.ToObject<GalleryImage>();
//            }
//        }
//
//        public override void WriteJson(JsonWriter writer, 
//            object value, JsonSerializer serializer)
//        {
//            throw new NotImplementedException();
//        }
//    }
}
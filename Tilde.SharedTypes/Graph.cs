// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tilde.SharedTypes
{
//    public struct Point
//    {
//        public int X;
//        public int Y; 
//    }
//        
//    public struct PointF
//    {
//        public float X;
//        public float Y; 
//    }
//
//    public struct Rectangle
//    {
//
//    }
    public struct Viewport
    {
        public int X, Y, Width, Height; 
    }
    
    public class Graph
    {
        public Viewport? Viewport;

        public List<GraphLine> Lines; 
    }
    
    public class GraphLine 
    {   
        [JsonProperty("style")]
        public string Style;
        
        [JsonProperty("path")] 
        public string Path;
    }
}
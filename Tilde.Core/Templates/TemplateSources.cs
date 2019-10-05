// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Tilde.Core.Projects;

namespace Tilde.Core.Templates
{
    public class TemplateSources : IDisposable
    {
        [JsonIgnore] 
        public DirectoryInfo PackageDirectory { get; }

        private string IndexFile => Path.Combine(PackageDirectory.FullName, "index.json"); 
        
        [JsonIgnore]
        public readonly ConcurrentDictionary<Uri, TemplateIndex> Sources = new ConcurrentDictionary<Uri, TemplateIndex>();
        
        private readonly EventDebouncer eventDebouncer;

        public TemplateSources(DirectoryInfo packageDirectory)
        {
            PackageDirectory = packageDirectory;
            
            //eventDebouncer = new EventDebouncer(Cache);

            Read(); 
        }

        public void Read()
        {
            if (File.Exists(IndexFile) == false)
            {
                return; 
            }

            using (StreamReader file = File.OpenText(IndexFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                
                Dictionary<Uri, TemplateIndex> fromFile = (Dictionary<Uri, TemplateIndex>)serializer.Deserialize(file, typeof(Dictionary<Uri, TemplateIndex>));

                foreach (Uri uri in Sources.Keys.Except(fromFile.Keys))
                {
                    Sources.TryRemove(uri, out _); 
                }

                foreach (var toAdd in fromFile)
                {
                    Sources[toAdd.Key] = toAdd.Value; 
                }
            }
        }

        public void Add(Uri uri)
        {
            Sources[uri] = null; 
        }

        public void Remove(Uri uri)
        {
            Sources.TryRemove(uri, out _);
        }


        public void Cache(Uri uri)
        {
            if (Sources.TryGetValue(uri, out TemplateIndex currentIndex) == false)
            {
                return; 
            }

            try
            {
                TemplateIndex index = ResourceHelper.GetResource<TemplateIndex>(uri);

                if (currentIndex != null && currentIndex.Hash.Equals(index.Hash))
                {
                    // Up to date.
                    Console.WriteLine($"{uri} [NO CHANGE]");
                        
                    return; 
                }
                    
                Console.WriteLine($"{uri} [{index.Hash}] [{currentIndex?.Hash ?? "NO PEER"}]");
                    
                Uri parent = new Uri(uri, "."); 

                TemplateIndex.Cache(PackageDirectory, parent, index, currentIndex);

                Sources[uri] = index; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cache template index. {uri}");
                Console.WriteLine(ex.Message);
            }
        }

        public void Cache()
        {
            foreach (Uri uri in Sources.Keys)
            {
                Cache(uri);
            }
        }

        // file:///D:/code/tilde-love/packing-test/index.json
        
        public void Write()
        {
            if (PackageDirectory.Exists == false)
            {
                PackageDirectory.Create();
            }

            using (StreamWriter file = File.CreateText(IndexFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                
                serializer.Formatting = Formatting.Indented;
                
                serializer.Serialize(file, Sources);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Write();
        }
    }
}
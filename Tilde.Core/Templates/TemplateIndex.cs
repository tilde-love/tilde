// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Tilde.Core.Templates
{
    /// <summary>
    ///     An index of templates.
    /// </summary>
    public class TemplateIndex
    {
        [JsonProperty("h")]
        public string Hash { get; set; }

        [JsonProperty("p")]
        public Dictionary<PackageName, string> Packages { get; set; } = new Dictionary<PackageName, string>();
        
        public string CalculateHash()
        {
            using (SHA256Managed sha = new SHA256Managed())
            using (MemoryStream resultStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(resultStream))
            {
                writer.WriteLine(Packages.Count);

                foreach (KeyValuePair<PackageName, string> entry in Packages.OrderBy(h => h.Key))
                {
                    writer.WriteLine(entry.Value);
                }

                writer.Flush();

                return Convert.ToBase64String(sha.ComputeHash(resultStream));
            }
        }

        public bool Validate()
        {
            return string.IsNullOrEmpty(Hash) == false && Hash.Equals(CalculateHash());
        }

        public static void Pack(DirectoryInfo source, DirectoryInfo destination)
        {
            TemplateIndex index = new TemplateIndex();
            
            foreach (DirectoryInfo packageFolder in source.GetDirectories())
            {
                (PackageName packageName, string hash) = Package.Pack(packageFolder, destination);

                index.Packages[packageName] = hash; 
                
                // Console.WriteLine($"{packageName} [{hash}]");
            }

            index.Hash = index.CalculateHash();

            string indexFilePath = Path.Combine(destination.FullName, "index.json"); 
            
            Console.WriteLine($"{indexFilePath} [{index.Hash}]");
            
            using (StreamWriter file = File.CreateText(indexFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, index);
            }
        }

        public static void Unpack(DirectoryInfo source, DirectoryInfo destination)
        {
            string indexFile = Path.Combine(source.FullName, "index.json"); 
            
            if (File.Exists(indexFile) == false)
            {
                Console.WriteLine($"{indexFile} does not exist."); 
                
                return; 
            }

            using (StreamReader file = File.OpenText(indexFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                
                TemplateIndex index = (TemplateIndex)serializer.Deserialize(file, typeof(TemplateIndex));
                
                Console.WriteLine($"{indexFile} [{index.Hash}] [NO PEER]");

                Cache(destination, new Uri($"file:///{source.FullName.Replace('\\', '/')}", UriKind.RelativeOrAbsolute), index, null); 
            }
        }

        public static void Cache(DirectoryInfo packageDirectory, Uri baseUri, TemplateIndex index, TemplateIndex existing)
        {
            foreach (PackageName packageName in index.Packages.Keys.Except((IEnumerable<PackageName>)existing?.Packages.Keys ?? new PackageName[0]))
            {
                Uri packageUri = new Uri(baseUri, $"{packageName.Name}.zip");
                
                if (index.Packages.TryGetValue(packageName, out var hash) == false)
                {
                    Console.WriteLine($" ¬ {packageUri} (NO HASH)"); 
                    
                    continue; 
                }

                Console.WriteLine($" ¬ {packageUri} [{hash}]"); 
                
                Package.Unpack(packageName, hash, ResourceHelper.GetResourceBytes(packageUri), packageDirectory);
            }
             
            // var CommonList = 
            foreach (PackageName packageName in index.Packages.Keys.Intersect((IEnumerable<PackageName>)existing?.Packages.Keys ?? new PackageName[0]))
            {
                Uri packageUri = new Uri(baseUri, $"{packageName.Name}.zip");
                
                if (index.Packages.TryGetValue(packageName, out var hash) == false)
                {
                    Console.WriteLine($" ¬ {packageUri} (NO HASH)"); 
                    
                    continue; 
                }
                
                if (existing.Packages.TryGetValue(packageName, out var existingHash) == true && 
                    string.Equals(hash, existingHash) == true)
                {
                    Console.WriteLine($" ¬ {packageUri} (NO CHANGE)"); 
                    
                    continue; 
                }
                
                Console.WriteLine($" ¬ {packageUri} [{hash}]"); 
                
                Package.Unpack(packageName, hash, ResourceHelper.GetResourceBytes(packageUri), packageDirectory);
            }
        }
    }
}
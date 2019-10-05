// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Tilde.Core.Templates
{
    public static class Package
    {
        public static string CalculateHash(
            PackageName name,
            Stream stream)
        {
            string contentHash;

            using (SHA256Managed sha = new SHA256Managed())
            {
                contentHash = Convert.ToBase64String(sha.ComputeHash(stream));
            }

            using (SHA256Managed sha = new SHA256Managed())
            using (MemoryStream resultStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(resultStream))
            {
                writer.WriteLine(name.ToString());
                writer.WriteLine(contentHash);
                writer.Flush();

                return Convert.ToBase64String(sha.ComputeHash(resultStream));
            }
        }

        public static (PackageName name, string hash) Pack(DirectoryInfo directory, DirectoryInfo outputFolder)
        {
            DirectoryInfo packageFolder = new DirectoryInfo(directory.FullName);

            if (PackageName.TryParse(packageFolder.Name, out PackageName packageName) == false)
            {
                throw new Exception($"Invalid package name {packageFolder.Name}");
            }

            FileInfo packageFileInfo = new FileInfo(Path.Combine(outputFolder.FullName, $"{packageName}.zip"));

            if (outputFolder.Exists == false)
            {
                outputFolder.Create();
            }

            if (packageFileInfo.Exists)
            {
                packageFileInfo.Delete();
            }
            
            Console.WriteLine($"{packageName.Name} {packageName.Version}");

            //ZipFile.CreateFromDirectory(directory.FullName, packageFileInfo.FullName, CompressionLevel.Optimal, false);

            long totalBytes = 0; 

            using (FileStream stream = new FileStream(packageFileInfo.FullName, FileMode.CreateNew, FileAccess.Write))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (FileInfo projectFile in packageFolder.GetFiles("*", SearchOption.AllDirectories))
                    {
                        Uri fileUri = new Uri(
                            projectFile.FullName
                                .Substring(packageFolder.FullName.Length)
                                .Replace('\\', '/')
                                .TrimStart('/'), 
                            UriKind.RelativeOrAbsolute
                        );

                        ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(fileUri.ToString(), CompressionLevel.Optimal);

                        using (Stream entryStream = zipArchiveEntry.Open())
                        {
                            using (FileStream fileStream = new FileStream(
                                projectFile.FullName,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite
                            ))
                            {
                                fileStream.CopyTo(entryStream);
                                
                                Console.WriteLine($" ¬ {fileUri} ({fileStream.Length} bytes)");

                                totalBytes += fileStream.Length;
                            }
                        }
                    }
                }
            }
            
            using (FileStream stream = new FileStream(packageFileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                string hash = CalculateHash(packageName, stream);
                    
                Console.WriteLine($"{packageFileInfo.FullName} ({stream.Length} bytes, {(float) stream.Length / (float) totalBytes:P2}) [{hash}]");
                
                return (packageName, hash);
            }
        }

        public static void Unpack(
            PackageName name,
            string hash,
            byte[] packageBytes,
            DirectoryInfo folder)
        {
            DirectoryInfo outputFolder = new DirectoryInfo(Path.Combine(folder.FullName, name.ToString()));

            using (MemoryStream stream = new MemoryStream(packageBytes))
            {
                if (Validate(name, stream, hash) == false)
                {
                    throw new Exception("Could not validate package.");
                }
            }
            
            using (MemoryStream stream = new MemoryStream(packageBytes))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                foreach (ZipArchiveEntry archiveEntry in archive.Entries)
                {
                    Uri filename = new Uri(archiveEntry.FullName, UriKind.RelativeOrAbsolute);

                    string filepath = Path.Combine(outputFolder.FullName, filename.ToString());
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filepath));

                    using (Stream archiveStream = archiveEntry.Open())
                    using (Stream fileStream = File.Open(
                        filepath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.Read
                    ))
                    {
                        archiveStream.CopyTo(fileStream);
                    }
                    
                    Console.WriteLine($"  ¬ {name}/{filename}"); 
                }
            }
        }

        public static bool Validate(PackageName name, Stream stream, string hash)
        {
            return string.IsNullOrEmpty(hash) == false && hash.Equals(CalculateHash(name, stream));
        }
    }
}
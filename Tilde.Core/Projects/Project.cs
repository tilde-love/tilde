// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tilde.Core.Controls;

namespace Tilde.Core.Projects
{
    public class Project : IComparable<Project>, IDisposable
    {
        [JsonIgnore] private static readonly List<string> CodeFileExtensions = new List<string> { ".cs", ".csproj" };
        
        [JsonIgnore] private static readonly List<string> DataFileExtensions = new List<string> { ".json", ".xml"};
        
        [JsonIgnore] private static readonly List<string> IgnoreFolders = new List<string> { "bin/", "obj/"};
        
        [JsonIgnore] private static readonly List<string> IgnoreFiles = new List<string> { "build", "log"};

        [JsonProperty("deleted")] public bool Deleted;

        [JsonProperty("errors")] public Dictionary<Uri, List<Error>> Errors = new Dictionary<Uri, List<Error>>();

        [JsonIgnore] public Dictionary<Uri, ProjectFile> Files = new Dictionary<Uri, ProjectFile>();

        [JsonIgnore] public readonly DirectoryInfo ProjectFolder;

        [JsonProperty("uri")] public Uri Uri;

        [JsonIgnore] public int Version = 0;

        [JsonIgnore] private readonly FileSystemWatcher allFilesWatcher;

        [JsonIgnore] private readonly AggregatedListDebouncer<Uri> fileDebouncer;

        [JsonProperty("files")]
        public IEnumerable<Uri> FileUris => Files.Keys;

        [JsonProperty("controls")]
        public ControlGroup Controls { get; }

        [JsonConstructor]
        internal Project()
        {

        }

        public Project(DirectoryInfo projectFolder)
        {
            ProjectFolder = projectFolder;

            Uri = new Uri(ProjectFolder.Name, UriKind.RelativeOrAbsolute);

            CreateFolder();

            fileDebouncer = new AggregatedListDebouncer<Uri>(UpdateFiles, 10, 100);

            allFilesWatcher = new FileSystemWatcher
            {
                Path = ProjectFolder.FullName,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = true
            };

            allFilesWatcher.Changed += OnAnyChanged;
            allFilesWatcher.Created += OnAnyChanged;
            allFilesWatcher.Deleted += OnDeleted;
            allFilesWatcher.Renamed += OnRenamed;
            
            Cache();
            
            Controls = new ControlGroup(this);
        }

        /// <inheritdoc />
        public int CompareTo(Project other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return Uri.Compare(
                Uri,
                other.Uri,
                UriComponents.Path,
                UriFormat.Unescaped,
                StringComparison.OrdinalIgnoreCase
            );
        }

        /// <inheritdoc />
        public void Dispose()
        {
            allFilesWatcher?.Dispose();
            
            fileDebouncer?.Dispose();
            
            Controls.Dispose();
        }

        public event EventHandler CodeFilesChanged;

        public event EventHandler DataFilesChanged;

        public void Delete()
        {
            if (ProjectFolder.Exists)
            {
                ProjectFolder.Delete(true);
            }
        }

        public bool DeleteFile(Uri filename)
        {
            try
            {
                if (File.Exists(GetFilePath(filename)))
                {
                    File.Delete(GetFilePath(filename));
                }

                ProjectChanged?.Invoke(this);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        public async Task<byte[]> Pack()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (Uri projectFile in Files.Keys)
                    {
                        ZipArchiveEntry zipArchiveEntry = archive.CreateEntry(projectFile.ToString());

                        using (Stream entryStream = zipArchiveEntry.Open())
                        {
                            byte[] bytes = ReadFileBytes(projectFile);

                            entryStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public event ProjectEvent ProjectChanged;
        public event FileEvent FileChanged;  

        public byte[] ReadFileBytes(Uri filename)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (FileStream fileStream = new FileStream(
                GetFilePath(filename),
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            ))
            {
                fileStream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
        }

        public string ReadFileString(Uri filename)
        {
            string filePath = GetFilePath(filename);

            if (File.Exists(filePath) == false)
            {
                return string.Empty;
            }

            using (FileStream fileStream = new FileStream(
                GetFilePath(filename),
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            ))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                return reader.ReadToEnd();
            }
        }

        public string TailLogFile()
        {
            string filePath = GetFilePath(new Uri("log", UriKind.RelativeOrAbsolute));

            if (File.Exists(filePath) == false)
            {
                return string.Empty;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileStream.Seek(-Math.Min(1024, fileStream.Length), SeekOrigin.End);

                if (fileStream.Position > 0)
                {
                    reader.ReadLine();
                }

                return reader.ReadToEnd();
            }
        }

        public async Task Unpack(byte[] bytes)
        {
            if (Directory.Exists(ProjectFolder.FullName))
            {
                ProjectFolder.Delete(true);
            }

            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        await WriteFile(new Uri(entry.FullName, UriKind.RelativeOrAbsolute), ReadEntryBytes(entry));
                    }
                }
            }
        }

        public async Task<bool> WriteFile(Uri filename, string contents)
        {
            return await WriteFile(filename, Encoding.UTF8.GetBytes(contents ?? string.Empty));
        }

        public async Task<bool> WriteFile(Uri filename, byte[] contents)
        {
            try
            {
                if (Files.TryGetValue(filename, out ProjectFile projectFile))
                {
                    string hash = GetHash(contents);

                    if (projectFile.Hash.Equals(hash))
                    {
                        // Debug.WriteLine($"{filename} HASH MATCH");

                        return true;
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(GetFilePath(filename)));

                using (Stream stream = File.Open(
                    GetFilePath(filename),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read
                ))
                {
                    await stream.WriteAsync(contents, 0, contents.Length);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        private void Cache()
        {
            Files = Directory.GetFiles(
                    ProjectFolder.FullName,
                    "*",
                    SearchOption.AllDirectories
                )
                .Where(
                    s => FilterFiles(s)
                )
                .Select(
                    s =>
                    {
                        Uri uri = new Uri(
                            s.Substring(ProjectFolder.FullName.Length + 1)
                                .Replace('\\', '/'),
                            UriKind.RelativeOrAbsolute
                        );

                        return new ProjectFile
                        {
                            Uri = uri,
                            Hash = GetFileHash(uri)
                        };
                    }
                )
                .ToDictionary(t => t.Uri, v => v);
        }

        private bool FilterFiles(string filepath)
        {
            if (filepath.StartsWith(ProjectFolder.FullName) == false)
            {
                return false; 
            }

            string localFilePath = filepath.Substring(ProjectFolder.FullName.Length + 1).Replace("\\", "/");

            if (IgnoreFiles.Contains(localFilePath))
            {
                return false;
            }
            
            foreach (string folder in IgnoreFolders)
            {
                if (localFilePath.StartsWith(folder) == true)
                {
                    return false; 
                }
            }

            return true; 
        }

        private void CreateFolder()
        {
            if (Directory.Exists(ProjectFolder.FullName) == false)
            {
                ProjectFolder.Create();
                
                File.WriteAllText(Path.Combine(ProjectFolder.FullName, "readme.md"), $"# {Uri}");

                string template = "basic-module";

                string templatePath = Path.Combine(new FileInfo(typeof(Project).Assembly.Location).DirectoryName, "templates", template); 
                
                File.WriteAllText(Path.Combine(ProjectFolder.FullName, "readme.md"), $"# {Uri}");

                string projectFile = Path.Combine(templatePath, $"{template}.csproj");
                
                File.Copy(projectFile, Path.Combine(ProjectFolder.FullName, $"{Uri}.csproj"));

                foreach (string file in Directory.GetFiles(templatePath, "*", SearchOption.AllDirectories))
                {
                    if (file == projectFile)
                    {
                        continue;
                    }

                    string localPath = file.Substring(templatePath.Length + 1);

                    string destinationPath = Path.Combine(ProjectFolder.FullName, localPath); 
                    
                    DirectoryInfo directoryInfo = new FileInfo(destinationPath).Directory;

                    if (directoryInfo?.Exists == false)
                    {
                        directoryInfo.Create();
                    }

                    File.Copy(file, destinationPath);
                }
            }
        }

        private string GetFileHash(Uri filename)
        {
            try
            {
                using (FileStream fileStream = new FileStream(
                    GetFilePath(filename),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                ))
                using (SHA256Managed sha = new SHA256Managed())
                {
                    return Convert.ToBase64String(sha.ComputeHash(fileStream));
                }
            }
            catch
            {
                return string.Empty; 
            }
        }

        private string GetFilePath(Uri filename)
        {
            return Path.Combine(ProjectFolder.FullName, filename.ToString());
        }

        private Uri GetFileUri(string fullName)
        {
            return new Uri(
                fullName.Substring(ProjectFolder.FullName.Length + 1)
                    .Replace('\\', '/'),
                UriKind.RelativeOrAbsolute
            );
        }

        private string GetHash(byte[] bytes)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }

        private void OnAnyChanged(object sender, FileSystemEventArgs e)
        {
            if (FilterFiles(e.FullPath) == false)
            {
                return; 
            }

            fileDebouncer.Invoke(GetFileUri(e.FullPath));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (FilterFiles(e.FullPath) == false)
            {
                return; 
            }

            Uri file = GetFileUri(e.FullPath);

            Files.Remove(file);

            FileChanged?.Invoke(Uri, file, null);
            
            Debug.WriteLine($"Deleted {Uri}/{file}");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Uri oldFile = GetFileUri(e.OldFullPath);
            Uri newFile = GetFileUri(e.FullPath);

            Files.Remove(oldFile);

            if (FilterFiles(e.FullPath) == false)
            {
                return; 
            }

            ProjectFile projectFile = new ProjectFile {Hash = GetFileHash(newFile), Uri = newFile};

            Files[newFile] = projectFile; 

            Debug.WriteLine($"Rename {Uri}/{oldFile} to {Uri}/{newFile}");

            FileChanged?.Invoke(Uri, oldFile, null);
            FileChanged?.Invoke(Uri, projectFile.Uri, projectFile.Hash);
            
            ProjectChanged?.Invoke(this);
        }

        private static byte[] ReadEntryBytes(ZipArchiveEntry entry)
        {
            using (Stream stream = entry.Open())
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private void UpdateFiles(Uri[] files)
        {
            bool anyChange = false;
            bool codeFileChanged = false;
            bool dataFileChanged = false;

            foreach (Uri file in files)
            {
                string filePath = GetFilePath(file);

                if (File.Exists(filePath) == false)
                {
                    continue;
                }

                string hash = GetFileHash(file);

                string extension = Path.GetExtension(file.ToString()); 

                if (Files.TryGetValue(file, out ProjectFile projectFile) == false)
                {
                    Debug.WriteLine($"New {Uri}/{file}");

                    Files[file] = new ProjectFile {Hash = hash, Uri = file};

                    anyChange = true;
                    codeFileChanged |= CodeFileExtensions.Contains(extension);
                    dataFileChanged |= DataFileExtensions.Contains(extension);

                    FileChanged?.Invoke(Uri, file, hash);
                    
                    continue;
                }

                if (projectFile.Hash.Equals(hash))
                {
                    Debug.WriteLine($"Unchanged {Uri}/{file}");

                    continue;
                }

                Files[file] = new ProjectFile {Hash = hash, Uri = file};

                anyChange = true;
                codeFileChanged |= CodeFileExtensions.Contains(extension);
                dataFileChanged |= DataFileExtensions.Contains(extension);

                Debug.WriteLine($"Changed {Uri}/{file}");
                
                FileChanged?.Invoke(Uri, file, hash);
            }

            if (codeFileChanged)
            {
                Debug.WriteLine($"Code files change detected {Uri}");
                CodeFilesChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (dataFileChanged)
            {
                Debug.WriteLine($"Data files change detected {Uri}");
                DataFilesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
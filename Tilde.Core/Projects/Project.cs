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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using Tilde.Core.Build;
using Tilde.Core.Controls;

namespace Tilde.Core.Projects
{
    public class Project : IComparable<Project>, IDisposable
    {
        [JsonProperty("deleted")] public bool Deleted;

        [JsonProperty("errors")] public Dictionary<Uri, List<Error>> Errors = new Dictionary<Uri, List<Error>>();

        [JsonIgnore] public Dictionary<Uri, ProjectFile> ProjectFiles = new Dictionary<Uri, ProjectFile>();

        [JsonIgnore] public readonly DirectoryInfo ProjectFolder;

        [JsonProperty("uri")] public Uri Uri;

        [JsonIgnore] public int Version = 0;

        [JsonIgnore] private readonly AggregatedListDebouncer<Uri> fileDebouncer;

        private Matcher ignore = new Matcher();
        private Matcher watch = new Matcher();

        [JsonProperty("controls")]
        public ControlGroup Controls { get; }

        [JsonProperty("definition")]
        public ProjectDefinition Definition { get; private set; }

        [JsonProperty("files")]
        public IEnumerable<ProjectFile> Files => ProjectFiles.Values.OrderBy(pf => pf.Uri.ToString());

        [JsonConstructor]
        internal Project()
        {
        }

        public Project(DirectoryInfo projectFolder)
        {
            ProjectFolder = new DirectoryInfo(projectFolder.FullName);

            Uri = new Uri(ProjectFolder.Name, UriKind.RelativeOrAbsolute);

            CreateFolder();

            fileDebouncer = new AggregatedListDebouncer<Uri>(UpdateFiles, 10, 100);

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
            fileDebouncer?.Dispose();

            Controls.Dispose();
        }

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

        public event FileEvent FileChanged;

        public string GetFilePath(Uri filename)
        {
            return Path.Combine(ProjectFolder.FullName, filename.ToString());
        }

        public Uri GetFileUri(string fullName)
        {
            return new Uri(
                fullName
                    .Substring(ProjectFolder.FullName.Length)
                    .Replace('\\', '/')
                    .TrimStart('/'),
                UriKind.RelativeOrAbsolute
            );
        }

        public async Task<byte[]> Pack()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (Uri projectFile in ProjectFiles.Keys)
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

        public IEnumerable<string> ReadFileLines(Uri filename)
        {
            string filePath = GetFilePath(filename);

            if (File.Exists(filePath) == false)
            {
                yield break;
            }

            using (FileStream fileStream = new FileStream(
                GetFilePath(filename),
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            ))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while (reader.EndOfStream == false)
                {
                    yield return reader.ReadLine();
                }
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

        public event EventHandler WatchFilesChanged;

        public async Task<bool> WriteFile(Uri filename, string contents)
        {
            return await WriteFile(filename, Encoding.UTF8.GetBytes(contents ?? string.Empty));
        }

        public async Task<bool> WriteFile(Uri filename, byte[] contents)
        {
            try
            {
                if (ProjectFiles.TryGetValue(filename, out ProjectFile projectFile))
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

        public void OnAnyChanged(object sender, FileSystemEventArgs e)
        {
            if (FilterFiles(e.FullPath) == false)
            {
                return;
            }

            fileDebouncer.Invoke(GetFileUri(e.FullPath));
        }

        public void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (FilterFiles(e.FullPath) == false)
            {
                return;
            }

            Uri file = GetFileUri(e.FullPath);

            ProjectFiles.Remove(file);

            FileChanged?.Invoke(Uri, file, null);

            Debug.WriteLine($"Deleted {Uri}/{file}");
        }

        public void OnRenamed(object sender, RenamedEventArgs e)
        {
            Uri oldFile = GetFileUri(e.OldFullPath);
            Uri newFile = GetFileUri(e.FullPath);

            ProjectFiles.Remove(oldFile);

            if (FilterFiles(e.FullPath) == false)
            {
                return;
            }

            ProjectFile projectFile = new ProjectFile {Hash = GetFileHash(newFile), Uri = newFile};

            ProjectFiles[newFile] = projectFile;

            Debug.WriteLine($"Rename {Uri}/{oldFile} to {Uri}/{newFile}");

            FileChanged?.Invoke(Uri, oldFile, null);
            FileChanged?.Invoke(Uri, projectFile.Uri, projectFile.Hash);

            ProjectChanged?.Invoke(this);
        }

        private void Cache()
        {
//            
//            Directory.GetFiles(
//                    ProjectFolder.FullName,
//                    "*",
//                    SearchOption.AllDirectories
//                )
//                .Where(FilterFiles)
//                

            ProcessTildeFile(new Uri("~project", UriKind.RelativeOrAbsolute));
            ProcessTildeFile(new Uri("~ignore", UriKind.RelativeOrAbsolute));
            ProcessTildeFile(new Uri("~watch", UriKind.RelativeOrAbsolute));

            ProjectFiles = Directory.GetFiles(
                    ProjectFolder.FullName,
                    "*",
                    SearchOption.AllDirectories
                )
                .Where(FilterFiles)
                // ProjectFiles = include.GetResultsInFullPath(ProjectFolder.FullName)
                .Select(
                    s =>
                    {
                        Uri uri = new Uri(
                            s.Substring(ProjectFolder.FullName.Length)
                                .Replace('\\', '/')
                                .TrimStart('/'),
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

        private void CreateFolder()
        {
            if (Directory.Exists(ProjectFolder.FullName))
            {
                return;
            }

            ProjectFolder.Create();
        }

        private bool FilterFiles(string filepath)
        {
            if (filepath.StartsWith(ProjectFolder.FullName) == false)
            {
                return false;
            }

            string localFilePath = filepath
                .Substring(ProjectFolder.FullName.Length)
                .Replace("\\", "/")
                .TrimStart('/');

            return ignore.Match(localFilePath)
                       .HasMatches == false;
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

        private string GetHash(byte[] bytes)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }

        private Matcher ParseGlobFile(Uri file, bool invertSelection)
        {
            Matcher matcher = new Matcher();

            foreach (string line in ReadFileLines(file))
            {
                if (line.Trim().Length == 0)
                {
                    continue;
                }

                if (line[0] == '#')
                {
                    continue;
                }

                string statement;
                bool include;

                if (line[0] == '!')
                {
                    include = false;
                    statement = line.Substring(1);
                }
                else
                {
                    include = true;
                    statement = line;
                }

                statement = statement.Trim(); 
                
                if (statement.Length == 0)
                {
                    continue;
                }

                include ^= invertSelection;

                if (include)
                {
                    matcher.AddInclude(statement);
                }
                else
                {
                    matcher.AddExclude(statement);
                }
            }

            return matcher;
        }

        private void ProcessTildeFile(Uri file)
        {
            switch (file.ToString())
            {
                case "~project":
                    try
                    {
                        Definition = JsonConvert.DeserializeObject<ProjectDefinition>(ReadFileString(file));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error parsing ~project");
                    }

                    break;
                case "~ignore":
                    try
                    {
                        Interlocked.Exchange(ref this.ignore, ParseGlobFile(file, false));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error parsing ~ignore");
                    }

                    break;
                case "~watch":
                    try
                    {
                        Interlocked.Exchange(ref watch, ParseGlobFile(file, false));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error parsing ~watch");
                    }

                    break;
            }
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
            bool watchedFileChanged = false;

            foreach (Uri file in files)
            {
                string filePath = GetFilePath(file);

                if (File.Exists(filePath) == false)
                {
                    continue;
                }

                string hash = GetFileHash(file);

                string extension = Path.GetExtension(file.ToString());

                if (ProjectFiles.TryGetValue(file, out ProjectFile projectFile) == false)
                {
                    Debug.WriteLine($"New {Uri}/{file}");

                    ProjectFiles[file] = new ProjectFile {Hash = hash, Uri = file};

                    anyChange = true;
                    watchedFileChanged |= watch.Match(filePath)
                        .HasMatches;

                    ProjectChanged?.Invoke(this);
                    FileChanged?.Invoke(Uri, file, hash);

                    continue;
                }

                if (projectFile.Hash.Equals(hash))
                {
                    Debug.WriteLine($"Unchanged {Uri}/{file}");

                    continue;
                }

                ProjectFiles[file] = new ProjectFile {Hash = hash, Uri = file};

                anyChange = true;

                watchedFileChanged |= watch.Match(filePath)
                    .HasMatches;

                Debug.WriteLine($"Changed {Uri}/{file}");

                FileChanged?.Invoke(Uri, file, hash);
            }

            if (watchedFileChanged)
            {
                Debug.WriteLine($"Watched files change detected {Uri}");
                WatchFilesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
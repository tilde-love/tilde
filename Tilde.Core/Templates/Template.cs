// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using Tilde.Core.Projects;

namespace Tilde.Core.Templates
{
    public class Template
    {
        public string Name { get; }

        public string Path { get; } 

        public void Instantiate(Uri projectName, string destination)
        {
            DirectoryInfo ProjectFolder = new DirectoryInfo(destination);
            
            if (Directory.Exists(ProjectFolder.FullName) == true)
            {
                return;
            }

            ProjectFolder.Create();

            File.WriteAllText(System.IO.Path.Combine(ProjectFolder.FullName, "readme.md"), $"# {projectName}");

            string projectFile = System.IO.Path.Combine(Path, $"project.csproj");

            File.Copy(projectFile, System.IO.Path.Combine(ProjectFolder.FullName, $"{projectName}.csproj"));

            foreach (string file in Directory.GetFiles(Path, "*", SearchOption.AllDirectories))
            {
                if (file == projectFile)
                {
                    continue;
                }

                string localPath = file.Substring(Path.Length + 1);

                string destinationPath = System.IO.Path.Combine(ProjectFolder.FullName, localPath);

                DirectoryInfo directoryInfo = new FileInfo(destinationPath).Directory;

                if (directoryInfo?.Exists == false)
                {
                    directoryInfo.Create();
                }

                File.Copy(file, destinationPath);
            }
        }
    }
}
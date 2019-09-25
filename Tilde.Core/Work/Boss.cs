// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Tilde.Core.Projects;

namespace Tilde.Core.Work
{
    public class Boss
    {
        private CancellationToken cancellationToken;
        
        public ConcurrentDictionary<string, Laborer> Work { get; set; } = new ConcurrentDictionary<string, Laborer>();

        public Boss()
        {

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;  
                
            using (CancellationTokenTaskSource<object> cancellationTokenTaskSource = new CancellationTokenTaskSource<object>(cancellationToken))
            {
                await cancellationTokenTaskSource.Task;
            }

            foreach (Laborer laborer in Work.Values.ToArray())
            {
                try
                {
                    laborer.Stop(CancellationToken.None); //  cancellationToken);
                }
                catch
                {

                }
                finally
                {
                    laborer.Dispose();
                }
            }
        }

        public Laborer Start(Project project, string name = null)
        {
            Laborer laborer = new Laborer()
            {
                Name = name ?? NameGenerator.Next(), 
                ProjectUri =  project.Uri, 
                Project = project, 
                RestartPolicy = LaborRestartPolicy.No, 
                Runner = new ProcessLaborer() 
                { 
                    Executable = project.Definition.Executable,
                    Arguments = project.Definition.Arguments,
                    WorkingDirectory = project.ProjectFolder.FullName,  // project.Definition.WorkingDirectory
                    Environment = new Dictionary<string, string>(),
                }
            };
            
            while (Work.TryAdd(laborer.Name, laborer) == false)
            {
                if (name != null)
                {
                    throw new Exception($"A laborer with the name {name} already exists.");
                }

                laborer.Name = NameGenerator.Next(true);
            }

            laborer.Run(cancellationToken);
            
            return laborer; 
        }
        
        public void Stop(string name)
        {
            if (Work.TryRemove(name, out Laborer laborer) == false)
            {
                return;
            }

            laborer.Stop(cancellationToken);
        }
    }
}
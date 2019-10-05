// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Tilde.Core.Projects;

namespace Tilde.Core.Work
{
    public class Boss
    {
        private CancellationToken cancellationToken;
        
        public ConcurrentDictionary<Uri, Laborer> Work { get; set; } = new ConcurrentDictionary<Uri, Laborer>();

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

                    laborer.StateChanged -= OnLaborerStateChanged;
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

        private void OnLaborerStateChanged(Laborer laborer)
        {
            WorkChanged?.Invoke(laborer); 
        }

        public Laborer RunProject(Project project, Uri name = null)
        {
            if (name != null)
            {
                TryRemove(name, out _);
            }

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

            Console.WriteLine($"RunProject {project.Uri} Laborer: {name}");

            Start(laborer.Name, out _);

            return laborer; 
        }
        
        public bool Start(Uri name, out Laborer laborer)
        {
            if (Work.TryGetValue(name, out laborer) == false)
            {
                return false;
            }

            Console.WriteLine($"Start Laborer {name}");
            
            laborer.Run(cancellationToken);
            
            WorkChanged?.Invoke(laborer);
            
            return true;
        }

        public bool Pause(Uri name, out Laborer laborer)
        {
            if (Work.TryGetValue(name, out laborer) == false)
            {
                return false;
            }

            Console.WriteLine($"Pause Laborer {name}");
            
            // laborer.Pause(cancellationToken);
            
            WorkChanged?.Invoke(laborer);
            
            return true;
        }
        
        public bool Stop(Uri name, out Laborer laborer)
        {
            if (Work.TryGetValue(name, out laborer) == false)
            {
                return false;
            }

            Console.WriteLine($"Stop Laborer {name}");
            
            laborer.Stop(cancellationToken);
            
            WorkChanged?.Invoke(laborer);
            
            return true;
        }

        public bool TryRemove(Uri name, out Laborer laborer)
        {
            laborer = null; 
            
            if (Stop(name, out _) == false)
            {
                return false;
            }

            if (Work.TryRemove(name, out laborer) == false)
            {
                return false;
            }

            Console.WriteLine($"Remove Laborer {name}");
            
            WorkChanged?.Invoke(laborer);
            
            return true;
        }

        public event Action<Laborer> WorkChanged;
    }
}
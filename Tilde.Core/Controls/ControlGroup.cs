// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tilde.Core.Projects;
using Tilde.SharedTypes;

namespace Tilde.Core.Controls
{
    public delegate void ControlValueEvent(Uri project, Uri control, string connectionId, object value); 
    public delegate void ControlPanelEvent(Uri project, Uri panelUri, ControlPanel panel);   
    public delegate void DataSourceEvent(Uri project, Uri uri, DataSource dataSource);
    
    public class ControlGroup : IDisposable
    {       
        [JsonProperty("values")]
        public readonly ConcurrentDictionary<Uri, object> Values = new ConcurrentDictionary<Uri, object>();
        
        [JsonProperty("panels")]
        public readonly ConcurrentDictionary<Uri, ControlPanel> Panels = new ConcurrentDictionary<Uri, ControlPanel>();
        
        [JsonProperty("sources")]
        public readonly ConcurrentDictionary<Uri, DataSource> Sources = new ConcurrentDictionary<Uri, DataSource>();
        
        public event ControlValueEvent ControlUpdated;
        public event ControlValueEvent ControlValueChanged;
        public event ControlPanelEvent ControlPanelChanged;
        public event DataSourceEvent DataSourceChanged;
        
        [JsonIgnore]
        public Project Project { get; }
        
        [JsonConstructor]
        internal ControlGroup()
        {

        }

        public ControlGroup(Project project)
        {
            Project = project;

            Project.FileChanged += OnFileChanged;

            foreach (ProjectFile file in project.Files.Values)
            {
                OnFileChanged(project.Uri, file.Uri, file.Hash);
            }
        }

        private void OnFileChanged(Uri project, Uri file, string hash)
        {
            string filepath = file.ToString();

            if (filepath.EndsWith(".panel.json") == false)
            {
                return; 
            }

            if (hash == null)
            {
                if (Panels.TryRemove(file, out ControlPanel _) == false)
                {
                    return; 
                }
                
                ControlPanelChanged?.Invoke(project, file, null);

                return; 
            }

            ControlPanel panel = null;  
            
            try
            {
                string fileString = Project.ReadFileString(file);

                List<Control> controls = JsonConvert.DeserializeObject<List<Control>>(fileString);

                panel = new ControlPanel {Hash = hash, Controls = controls};

                Panels[file] = panel; 
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error parsing panel");
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                ControlPanelChanged?.Invoke(project, file, panel);
            }
        }

        public async Task SetValue(Uri uri, string connectionId, object value)
        {
            Values[uri] = value; 
         
            ControlValueChanged?.Invoke(Project.Uri, uri, connectionId, value);
        }
        
        public async Task DefineDataSource(
            Uri uri,
            DataSourceType dataSourceType,
            bool @readonly,
            NumericRange? range,
            string[] values,
            Graph graph)
        {
            DataSource dataSource = new DataSource
            {
                Uri = uri,
                DataSourceType = dataSourceType,
                Readonly = @readonly,
                NumericRange = range,
                Values = values,
                Graph = graph 
            };

            Sources[uri] = dataSource;
         
            DataSourceChanged?.Invoke(Project.Uri, uri, dataSource);
        }     
        
        public async Task DeleteDataSource(Uri uri)
        {
            if (Sources.TryRemove(uri, out DataSource _) == false)
            {
                return; 
            }

            DataSourceChanged?.Invoke(Project.Uri, uri, null);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Project.FileChanged -= OnFileChanged;
        }

        public async Task UpdateValue(Uri uri, string connectionId, object value)
        {
            ControlUpdated?.Invoke(Project.Uri, uri, connectionId, value);
        }
    }
}
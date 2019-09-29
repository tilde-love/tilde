using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tilde.Core.Projects;

namespace Tilde.Core.Work
{
    [JsonConverter(typeof(LaborRunnerConverter))]
    public interface ILaborRunner
    {
        [JsonProperty("type")]
        string Type { get; }
        
        [JsonProperty("state")]
        LaborState State { get; set; }

        Task<(int? exitCode, string message)> Work(Project project, CancellationToken cancellationToken);
    }
}
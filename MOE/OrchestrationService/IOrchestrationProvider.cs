using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MOE.OrchestrationService
{
    public interface IOrchestrationProvider
    {
        IEnumerable<string> GetAll();
        void Reload();
        Task<object> Start(string orchestratorName, Dictionary<string, object> args);
    }
}

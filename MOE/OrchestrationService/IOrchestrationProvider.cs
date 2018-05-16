using System;
using System.Collections.Generic;
using System.Text;

namespace MOE.OrchestrationService
{
    public interface IOrchestrationProvider
    {
        IEnumerable<string> GetAll();
        void Reload();
        object Start(string orchestratorName, Dictionary<string, object> args);
    }
}

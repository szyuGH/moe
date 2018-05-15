using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MOE.OrchestrationService
{
    public class OrchestrationProvider : IOrchestrationProvider
    {
        private ConcurrentDictionary<string, Orchestrator> orchestrators = new ConcurrentDictionary<string, Orchestrator>();

        public IEnumerable<string> GetAll()
        {
            return orchestrators.Keys;
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public object Start(string orchestratorName, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}

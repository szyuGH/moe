using System;
using System.Collections.Generic;
using System.Text;

namespace MOE
{
    public class Orchestrator
    {
        public string Name;
        public Version Version;
        public Type OutputType;
        public Dictionary<string, string> Input;
        public List<ServiceCall> ServiceCalls;
        public List<OrchestrationEvent> Events;
    }
}

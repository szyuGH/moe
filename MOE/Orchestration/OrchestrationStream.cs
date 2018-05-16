using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MOE
{
    public class OrchestrationStream
    {
        public Orchestrator Definition;
        public object Result { get; private set; }

        private ConcurrentDictionary<string, object> stack;
        private int state;

        public OrchestrationStream(Orchestrator orchestrator, Dictionary<string, object> args)
        {
            Definition = orchestrator;
            stack = new ConcurrentDictionary<string, object>();
            state = 0;

            foreach (KeyValuePair<string, object> kvp in args)
            {
                stack[kvp.Key] = kvp.Value;
            }
            
        }
        
        public void Run()
        {
            foreach (ServiceCall sc in Definition.ServiceCalls)
            {
                HttpClient req = new HttpClient(                    );
                
            }
        }


    }
}

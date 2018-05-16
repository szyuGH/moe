using System;
using System.Collections.Generic;
using System.Text;

namespace MOE
{
    public class OrchestrationEvent
    {
        public int Id;
        public string Event;
        public object Result;
        public ResultEventType Type;

        public enum ResultEventType
        {
            CONTINUE, RETURN
        }
    }
}

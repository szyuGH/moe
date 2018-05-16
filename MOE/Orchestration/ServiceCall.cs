using System;
using System.Collections.Generic;
using System.Text;

namespace MOE
{
    public class ServiceCall
    {
        public int Id;
        public string Resource;
        public ServiceCallType Type;
        public string[] InputRedirect;
        public Tuple<string, string> StackOutput;


        public enum ServiceCallType
        {
            GET, POST
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        
        public async void Run()
        {
            foreach (ServiceCall sc in Definition.ServiceCalls)
            {
                string uri = sc.Resource;
                HttpContent content = null;
                HttpStatusCode statusCode = HttpStatusCode.BadRequest;
                if (sc.Type == ServiceCall.ServiceCallType.GET)
                {
                    if (uri[uri.Length - 1] != '/')
                        uri += '/';

                    foreach (string ir in sc.InputRedirect)
                    {
                        uri += stack[ir] + "/";
                    }


                    HttpClient client = new HttpClient();
                    
                    var r = await client.GetAsync(uri);
                    statusCode = r.StatusCode;
                    content = r.Content;
                } else if (sc.Type == ServiceCall.ServiceCallType.POST)
                {
                    StringBuilder paramString = new StringBuilder();
                    paramString.AppendLine("{");
                    for (int i=0;i<sc.InputRedirect.Length;i++)
                    {
                        string ir = sc.InputRedirect[i];
                        paramString.Append($"\"{ir}\": {StackValToBodyParam(stack[ir])}");
                        if (i < sc.InputRedirect.Length - 1)
                            paramString.Append(",");
                        paramString.AppendLine();
                    }
                    paramString.AppendLine("}");

                    HttpClient client = new HttpClient();
                    HttpContent httpContent = new StringContent(paramString.ToString(), Encoding.UTF8, "application/json");
                    var r = await client.PostAsync(uri, httpContent);
                    statusCode = r.StatusCode;
                    content = r.Content;
                }
                
                if (content != null && statusCode == HttpStatusCode.OK)
                {
                    string responseString = await content.ReadAsStringAsync();
                    if (responseString.Length > 0)
                    {
                        if (responseString[0] == '{' && responseString[responseString.Length - 1] == '}')
                            stack[sc.StackOutput.Item1] = JsonConvert.DeserializeObject(responseString);
                        else
                            stack[sc.StackOutput.Item1] = Convert.ChangeType(responseString, Type.GetType(sc.StackOutput.Item2), CultureInfo.InvariantCulture);
                    }
                } // TODO: Error management
            }
        }

        private string StackValToBodyParam(object o)
        {
            if (o == null)
                return "null";
            else if (o is string)
                return o == null ? "null" : $"\"{o}\"";
            else if (o.IsNumber())
                return o.ToString();
            else if (o is DateTime)
                return ((DateTime)o).ToBinary().ToString();
            else return o.ToString();
        }
    }
}

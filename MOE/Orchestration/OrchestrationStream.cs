using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        
        public async Task<object> Run()
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
                } 
                
                foreach (OrchestrationEvent ev in Definition.Events.Where(e => e.Id == sc.Id))
                {
                    bool conditionMet = false;
                    if (ev.Event == "FAIL")
                    {
                        conditionMet = statusCode != HttpStatusCode.OK;
                    } else if (ev.Event.StartsWith('$') && ev.Event.EndsWith('$'))
                    {
                        Match match = Regex.Match(ev.Event, @"\$(.+)\s*(={2}|>=|<=|<|>|!=)\s*(.+)\$");
                        if (match.Success)
                        {
                            Type leftType;
                            Type rightType;
                            object left = match.Groups[1].Value.Trim();
                            object op = match.Groups[2].Value.Trim();
                            object right = match.Groups[3].Value.Trim();

                            if ((left as string).StartsWith('{') && (left as string).EndsWith('}'))
                            {
                                left = stack[(left as string).Substring(1, (left as string).Length - 2)];
                                leftType = left.GetType();
                            } else
                            {
                                string[] split = (left as string).Split(':');
                                leftType = Type.GetType(split[1]);
                                left = Convert.ChangeType(split[0], leftType);
                            }
                            if ((right as string).StartsWith('{') && (right as string).EndsWith('}'))
                            {
                                right = stack[(right as string).Substring(1, (right as string).Length - 2)];
                                rightType = right.GetType();
                            } else
                            {
                                string[] split = (right as string).Split(':');
                                rightType = Type.GetType(split[1]);
                                right = Convert.ChangeType(split[0], rightType);
                            }
                            switch (op)
                            {
                                case "==": conditionMet = left.Equals(right);
                                    break;
                                case ">=":
                                    conditionMet = (left is IComparable && right is IComparable) ? (left as IComparable).CompareTo(right as IComparable) >= 0 : false;
                                    break;
                                case "<=":
                                    conditionMet = (left is IComparable && right is IComparable) ? (left as IComparable).CompareTo(right as IComparable) <= 0 : false;
                                    break;
                                case "!=":
                                    conditionMet = !left.Equals(right);
                                    break;
                                case "<":
                                    conditionMet = (left is IComparable && right is IComparable) ? (left as IComparable).CompareTo(right as IComparable) < 0 : false;
                                    break;
                                case ">":
                                    conditionMet = (left is IComparable && right is IComparable) ? (left as IComparable).CompareTo(right as IComparable) > 0 : false;
                                    break;
                            }
                        }
                    }

                    if (conditionMet)
                    {
                        Result = Convert.ChangeType(ev.Result, Definition.OutputType);
                        if (ev.Type == OrchestrationEvent.ResultEventType.RETURN)
                        {
                            return Result;
                        }
                    }
                }
            }

            return Result;
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

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
        public Orchestrator Definition { get; private set; }
        public object Result { get; private set; }

        private ConcurrentDictionary<string, object> paramBag; // used to manage all variables during a stream
        private int currentServiceId;

        public OrchestrationStream(Orchestrator orchestrator, Dictionary<string, object> args)
        {
            Definition = orchestrator;
            paramBag = new ConcurrentDictionary<string, object>();
            currentServiceId = 0;

            // pre initialize bag with existing values from input
            foreach (KeyValuePair<string, object> kvp in args)
            {
                paramBag[kvp.Key] = kvp.Value;
            }
        }
        
        public async Task<object> Run()
        {
            foreach (ServiceCall sc in Definition.ServiceCalls)
            {
                HttpResponseMessage response = await SendHttpMessage(sc);
                if (response == null)
                {
                    Console.WriteLine("Not supported http method for service call with id " + sc.Id);
                    return null;
                }
                
                if (response.Content != null && response.StatusCode == HttpStatusCode.OK)
                {
                    // read response and update paramBag
                    string responseString = await response.Content.ReadAsStringAsync();
                    if (responseString.Length > 0)
                    {
                        try
                        {
                            if (responseString[0] == '{' && responseString[responseString.Length - 1] == '}')
                                paramBag[sc.StackOutput.Item1] = JsonConvert.DeserializeObject(responseString);
                            else
                                paramBag[sc.StackOutput.Item1] = Convert.ChangeType(responseString, Type.GetType(sc.StackOutput.Item2), CultureInfo.InvariantCulture);
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Could not update Parameter Bag:");
                            Console.WriteLine(e);
                            Console.ResetColor();
                        }
                    }
                } 
                
                foreach (OrchestrationEvent ev in Definition.Events.Where(e => e.Id == sc.Id))
                {
                    bool conditionMet = false;
                    if (ev.Event == "FAIL")
                    {
                        conditionMet = response.StatusCode != HttpStatusCode.OK;
                    }
                    else if (ev.Event.StartsWith('$') && ev.Event.EndsWith('$'))
                    {
                        Match match = Regex.Match(ev.Event, @"\$(.+)\s*(={2}|>=|<=|<|>|!=)\s*(.+)\$");
                        if (match.Success)
                        {
                            
                            string leftExpr = match.Groups[1].Value.Trim();
                            string op = match.Groups[2].Value.Trim();
                            string rightExpr = match.Groups[3].Value.Trim();

                            conditionMet = EvaluateCondition(leftExpr, rightExpr, op);
                        }
                    }

                    // if condition is met, decide for return or further computation
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

        private async Task<HttpResponseMessage> SendHttpMessage(ServiceCall sc)
        {
            string uri = sc.Resource;
            HttpClient client = new HttpClient();

            if (sc.Type == ServiceCall.ServiceCallType.GET)
            {
                // ensure slash at the end
                if (uri[uri.Length - 1] != '/')
                    uri += '/';

                // append all parameters
                foreach (string ir in sc.InputRedirect)
                {
                    uri += paramBag[ir] + "/";
                }
                return await client.GetAsync(uri);
            }
            else if (sc.Type == ServiceCall.ServiceCallType.POST)
            {
                // create json body
                StringBuilder paramString = new StringBuilder();
                paramString.AppendLine("{");
                for (int i = 0; i < sc.InputRedirect.Length; i++)
                {
                    string ir = sc.InputRedirect[i];
                    paramString.Append($"\"{ir}\": {StackValToBodyParam(paramBag[ir])}");
                    if (i < sc.InputRedirect.Length - 1)
                        paramString.Append(",");
                    paramString.AppendLine();
                }
                paramString.AppendLine("}");
                
                HttpContent httpContent = new StringContent(paramString.ToString(), Encoding.UTF8, "application/json");
                return await client.PostAsync(uri, httpContent);
            }
            return null;
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

        private bool EvaluateCondition(string left, string right, string op)
        {
            object leftVal = ConvertFromString(left);
            object rightVal = ConvertFromString(right);

            // both values must be IComparable so comparison is possible
            if (!(leftVal is IComparable) && !(rightVal is IComparable))
                return false;
            
            switch (op)
            {
                case "==":
                    return leftVal.Equals(rightVal);
                case "!=":
                    return !leftVal.Equals(right);
                case ">=":
                    return (leftVal as IComparable).CompareTo(rightVal as IComparable) >= 0;
                case "<=":
                    return (leftVal as IComparable).CompareTo(rightVal as IComparable) <= 0;
                case "<":
                    return (leftVal as IComparable).CompareTo(rightVal as IComparable) < 0;
                case ">":
                    return (leftVal as IComparable).CompareTo(rightVal as IComparable) > 0;
            }
            return false;
        }

        private object ConvertFromString(string expr)
        {
            object val;
            if (expr.StartsWith('{') && expr.EndsWith('}'))
            {
                val = paramBag[expr.Substring(1, expr.Length - 2)];
            }
            else if (expr.Contains(":"))
            {
                string[] split = expr.Split(':');
                val = Convert.ChangeType(split[0], Type.GetType(split[1]));
            } else
            {
                val = expr;
            }

            return val;
        }
    }
}

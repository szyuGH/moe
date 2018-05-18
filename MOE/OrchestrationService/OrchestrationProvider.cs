using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace MOE.OrchestrationService
{
    public class OrchestrationProvider : IOrchestrationProvider
    {
        private ConcurrentDictionary<string, Orchestrator> orchestrators;
        private System.Timers.Timer timer;
        private List<OrchestrationStream> currentStreams;

        public OrchestrationProvider()
        {
            orchestrators = new ConcurrentDictionary<string, Orchestrator>();
            currentStreams = new List<OrchestrationStream>();
            Reload();

            timer = new System.Timers.Timer(2000);
            timer.AutoReset = true;
            timer.Elapsed += (sender, e) => { Reload(); };
            timer.Start();
        }
        ~OrchestrationProvider()
        {
            timer.Stop();
            timer.Dispose();
        }

        public IEnumerable<string> GetAll()
        {
            return orchestrators.Keys;
        }

        public void Reload()
        {
            foreach (string osFile in Directory.EnumerateFiles("Orchestrated Services", "*.json"))
            {
                try
                {
                    Orchestrator o = JsonConvert.DeserializeObject<Orchestrator>(File.ReadAllText(osFile));
                    o.ServiceCalls.Sort((s1, s2) => s1.Id.CompareTo(s2.Id));
                    if (!orchestrators.ContainsKey(o.Name) || !orchestrators[o.Name].Version.Equals(o.Version))
                    {
                        orchestrators[o.Name] = o;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Updated Orchestrated Service Definition: {o.Name} [{o.Version}]");
                        Console.ResetColor();
                    }
                }catch (Exception)
                {
                    Console.WriteLine("Error while reading Orchestrated Service Definition: " + osFile);
                }
            }
        }

        public object Start(string orchestratorName, Dictionary<string, object> args)
        {
            if (!orchestrators.ContainsKey(orchestratorName))
                return null;
            OrchestrationStream oStream = new OrchestrationStream(orchestrators[orchestratorName], args);
            currentStreams.Add(oStream);

            oStream.Run();

            return oStream.Result;
        }
        
    }
}

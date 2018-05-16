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
        private ConcurrentDictionary<string, Orchestrator> orchestrators = new ConcurrentDictionary<string, Orchestrator>();
        private System.Timers.Timer timer;

        public OrchestrationProvider()
        {
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

        public object Start(string orchestratorName, params object[] args)
        {
            throw new NotImplementedException();
        }
        
    }
}

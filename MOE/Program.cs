using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MOE.OrchestrationService;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace MOE
{
    class Program
    {

        static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Services.GetService<IOrchestrationProvider>();
            host.Run();
        }
        

        public static IWebHost BuildWebHost(string[] args) {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .ConfigureServices(s =>
                {
                    s.AddMvc();
                    s.AddSingleton<IOrchestrationProvider, OrchestrationProvider>();
                })
                .Configure(app =>
                {
                    app.UseMvcWithDefaultRoute();
                })
                .Build();
        }



    }
}

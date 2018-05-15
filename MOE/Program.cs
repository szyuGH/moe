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

namespace MOE
{
    class Program
    {
        static Timer timer;

        static void Main(string[] args)
        {
            timer = new System.Timers.Timer(2000);
            timer.AutoReset = true;
            timer.Elapsed += T_Elapsed;
            timer.Start();

            //BuildWebHost(args).Run();

            timer.Stop();
            timer.Dispose();
        }
        

        public static IWebHost BuildWebHost(string[] args) {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .ConfigureServices(s =>
                {
                    s.AddMvc();
                })
                .Configure(app =>
                {
                    app.UseMvcWithDefaultRoute();
                })
                .Build();
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Tick");
        }
    }
}

﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CustomerManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5010")
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
    }
}

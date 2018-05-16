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



        static Orchestrator CreateExampleOrchestrator()
        {
            return new Orchestrator()
            {
                Name = "Payment_OS",
                Version = new Version("1.0.0.0"),
                OutputType = typeof(bool),
                Input = new Dictionary<string, string>()
                {
                    { "cid", typeof(string).FullName },
                    { "pin", typeof(string).FullName },
                    { "discountCount", typeof(string).FullName },
                    { "price", typeof(float).FullName },
                    { "orderCount", typeof(int).FullName },
                },
                ServiceCalls = new List<ServiceCall>()
                {
                    new ServiceCall()
                    {
                        Id = 0,
                        Resource = "http://localhost:5010/api/customer/auth",
                        Type = ServiceCall.ServiceCallType.POST,
                        InputRedirect = new string[]{ "cid", "pin" },
                        StackOutput = new Tuple<string, string>("iban", typeof(string).FullName)
                    },
                    new ServiceCall()
                    {
                        Id = 1,
                        Resource = "http://localhost:5050/api/discounts",
                        Type = ServiceCall.ServiceCallType.GET,
                        InputRedirect = new string[]{ "discountCode", "price" },
                        StackOutput = new Tuple<string, string>("price", typeof(float).FullName)
                    },
                    new ServiceCall()
                    {
                        Id = 2,
                        Resource = "http://localhost:5020/api/payment/pay",
                        Type = ServiceCall.ServiceCallType.POST,
                        InputRedirect = new string[]{ "iban", "price" },
                        StackOutput = new Tuple<string, string>("payReturnCode", typeof(int).FullName)
                    },
                    new ServiceCall()
                    {
                        Id = 3,
                        Resource = "http://localhost:5010/api/customer/updateorders",
                        Type = ServiceCall.ServiceCallType.POST,
                        InputRedirect = new string[]{ "cid", "orderCount" },
                        StackOutput = new Tuple<string, string>("orderCount", typeof(int).FullName)
                    },
                    new ServiceCall()
                    {
                        Id = 4,
                        Resource = "http://localhost:5010/api/customer/bonus",
                        Type = ServiceCall.ServiceCallType.POST,
                        InputRedirect = new string[]{ "cid" },
                        StackOutput = null
                    },
                },
                Events = new List<OrchestrationEvent>()
                {
                    new OrchestrationEvent()
                    {
                        Id = 0,
                        Event = "FAIL",
                        Type = OrchestrationEvent.ResultEventType.RETURN,
                        Result = false
                    },
                    new OrchestrationEvent()
                    {
                        Id = 1,
                        Event = "FAIL",
                        Type = OrchestrationEvent.ResultEventType.RETURN,
                        Result = false
                    },
                    new OrchestrationEvent()
                    {
                        Id = 2,
                        Event = "FAIL",
                        Type = OrchestrationEvent.ResultEventType.RETURN,
                        Result = false
                    },
                    new OrchestrationEvent()
                    {
                        Id = 2,
                        Event = "{payReturnCode} == 0",
                        Type = OrchestrationEvent.ResultEventType.CONTINUE,
                        Result = true
                    }
                }
            };
        }
    }
}

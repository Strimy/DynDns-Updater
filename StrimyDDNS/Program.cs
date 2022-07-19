using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

namespace StrimyDDNS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSystemd()
                .ConfigureLogging((ctx,logging) =>
                {
                    logging.AddEventLog();
                })
                .ConfigureServices((ctx, services)=>
                {
                    services.Configure<GlobalConfig>(ctx.Configuration);
                    services.AddHostedService<DynDnsService>();
                })
                .Build()
                .Run()
                ;
        }
    }
}
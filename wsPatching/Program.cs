using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutomationStandards
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                   
                    webBuilder.UseStartup<Startup>();
                })
                //2020.05.28 - adding config section for env variables/containers
                .ConfigureAppConfiguration(appConf =>
                {
                    appConf.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), false, true);
                    appConf.AddEnvironmentVariables(); //this order should allow environment variables to take precedence (if present)
                });
    }
}

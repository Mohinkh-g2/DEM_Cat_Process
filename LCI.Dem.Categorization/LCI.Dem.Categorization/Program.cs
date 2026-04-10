using LCI.Dem.Categorization.Infrastructure.Configs;
using LCI.Dem.Categorization.Infrastructure.Extensions;
using LCI.Dem.Categorization.Workers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace LCI.Dem.Categorization
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Microsoft.Extensions.Logging.ILogger logger = null;
            //System.Diagnostics.Debugger.Launch();
            var builder = CreateHostBuilder(args).Build();
            logger = builder.Services.GetService<ILogger<Program>>();
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
            try
            {
                logger.LogInformation("Starting web host");
                builder.Run();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Host unexpectedly terminated");
            }
        }
               

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseEnvironment(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            .ConfigureHostConfiguration(configBuilder =>
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (string.IsNullOrEmpty(environment) || environment.Equals("Development"))
                {
                    configBuilder.AddJsonFile("appsettings.json", false, true);
                    configBuilder.AddJsonFile("appsettings.Logs.json", true, true);
                }
                else
                {
                    configBuilder.AddJsonFile($"appsettings.{environment}.json", false, true);
                    configBuilder.AddJsonFile($"appsettings.Logs.{environment}.json", true, true);
                }
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<CategorizationService>();
                services.AddHostedService<CategorizationServiceRerun>();
                var sp = services.BuildServiceProvider();
                var config = sp.GetService<IConfiguration>();

                services.AddAutoMapper(typeof(MappingProfileConfiguration));
                services.AddServicesInAssembly(config);
                services.AddMemoryCache();
            })
            .UseWindowsService(options =>
            {
                options.ServiceName = "DEM Categorization";
            })
            .UseSerilog((hostingContext, loggerConfig) =>
                    loggerConfig.ReadFrom.Configuration(hostingContext.Configuration)
                );
        }
    }
}

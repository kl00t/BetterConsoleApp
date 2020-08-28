using System;
using System.IO;
using ConsoleUI.Interfaces;
using ConsoleUI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ConsoleUI
{
    public class Program
    {
        public static void Main()
        {
           var configurationBuilder = new ConfigurationBuilder();
           BuildConfig(configurationBuilder);

           // Configure Serilog
           Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(configurationBuilder.Build())
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .CreateLogger();

           Log.Logger.Information("Application Starting");

           var host = Host.CreateDefaultBuilder()
               .ConfigureServices((context, services) =>
               {
                    services.AddTransient<IProductService, ProductService>();
                    services.AddTransient<IGreetingService, GreetingService>(); // Transient - Give me a new service each time I request it.
               })
               .UseSerilog()
               .Build();

            var service = ActivatorUtilities.CreateInstance<GreetingService>(host.Services);
            service.Run();

            //var services = ActivatorUtilities.CreateInstance<ProductService>(host.Services);
            //services.RunAsync().GetAwaiter().GetResult();
        }

        static void BuildConfig(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
    }
}

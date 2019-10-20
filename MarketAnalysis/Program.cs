using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SerilogTimings;
using System;
using System.ServiceProcess;

namespace MarketAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RegisterLogger();
                using (var op = Operation.Begin("Performing market analysis"))
                using (var provider = RegisterServices())
                {
                    if (Environment.UserInteractive)
                    {
                        var service = provider.GetService<AnalysisService>();
                        service.Execute().Wait();
                    }
                    else
                    {
                        using (var service = provider.GetService<WindowsService>())
                        {
                            ServiceBase.Run(service);
                        }
                    }
                    op.Complete();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
                throw;
            }
        }

        public static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(IProvider), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository), typeof(FileRepository));
            services.AddSingleton(typeof(IApiClient), typeof(ApiMarketDataProvider));
            services.AddSingleton(typeof(ICommunicationService), typeof(EmailCommunicationService));
            services.AddTransient(typeof(WindowsService), typeof(WindowsService));
            services.AddTransient<AnalysisService>();

            return services.BuildServiceProvider();
        }

        public static void RegisterLogger()
        {
            var logPath = Configuration.LogPath;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath)
                .CreateLogger();
        }
    }
}

using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Services;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SerilogTimings;
using System;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                RegisterLogger();
                using (var op = Operation.Begin("Performing market analysis"))
                using (var provider = RegisterServices())
                {
                    var service = provider.GetService<AnalysisService>();
                    await service.Execute();
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

            // Repositories
            services.AddSingleton(typeof(IRepository<MarketData>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<IStrategy>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<EmailTemplate>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<SimulationResult>), typeof(FileRepository));

            // Providers
            services.AddSingleton<ProgressBarProvider>();
            services.AddSingleton(typeof(IResultsProvider), typeof(ResultsProvider));
            services.AddSingleton<ApiMarketDataProvider>();
            services.AddSingleton<StrategyProvider>();
            services.AddSingleton<MarketDataProvider>();
            services.AddSingleton<EmailTemplateProvider>();

            // Caches
            services.AddSingleton(typeof(MarketDataCache), MarketDataCache.Instance);
            services.AddSingleton<SimulationCache>();

            // Services
            services.AddSingleton(typeof(ISimulator), typeof(Simulator));
            services.AddSingleton(typeof(ICommunicationService), typeof(EmailCommunicationService));
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

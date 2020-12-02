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
                    op.Complete();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception occured:");
                throw;
            }
        }

        public static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            // Caches
            services.AddSingleton<MarketDataCache>();
            services.AddSingleton<SimulationCache>();

            // Repositories
            services.AddSingleton(typeof(IRepository<Investor>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<IStrategy>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<MarketData>), typeof(FileRepository));
            services.AddSingleton(typeof(IRepository<SimulationResult>), typeof(FileRepository));

            // Providers
            services.AddSingleton<ProgressBarProvider>();
            services.AddSingleton(typeof(IResultsProvider), typeof(ResultsProvider));
            //services.AddSingleton<IApiDataProvider, WorldTradingDataProvider>();
            //services.AddSingleton<IApiDataProvider, AlphaVantageDataProvider>();
            services.AddSingleton<IApiDataProvider, YahooFinanceProvider>();
            services.AddSingleton<StrategyProvider>();
            services.AddSingleton<InvestorProvider>();
            services.AddSingleton<MarketDataProvider>();
            services.AddSingleton<ReportProvider>();

            // Services
            services.AddSingleton(typeof(ISimulator), typeof(Simulator));
            services.AddSingleton(typeof(ICommunicationService), typeof(EmailCommunicationService));
            services.AddTransient<AnalysisService>();

            return services.BuildServiceProvider();
        }

        public static void RegisterLogger()
        {
            var logPath = DirectoryManager.GetLatestLogFile();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath)
                .CreateLogger();
        }
    }
}

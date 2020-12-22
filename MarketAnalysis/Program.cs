using MarketAnalysis.Caching;
using MarketAnalysis.Factories;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using MarketAnalysis.Services;
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
                using var op = Operation.Begin("Performing market analysis");
                await using var provider = RegisterServices();
                var service = provider.GetService<AnalysisService>();
                await service.Execute();
                op.Complete();
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
            services.AddSingleton<IMarketDataCache, MarketDataCache>();
            services.AddSingleton<ISimulationCache, SimulationCache>();

            // Repositories
            services.AddSingleton<IRepository<MarketData>, FileRepository>();
            services.AddSingleton<IRepository<SimulationResult>, FileRepository>();

            // Providers
            services.AddSingleton<ProgressBarProvider>();
            services.AddSingleton<IResultsProvider, ResultsProvider>();
            //services.AddSingleton<IApiDataProvider, WorldTradingDataProvider>();
            //services.AddSingleton<IApiDataProvider, AlphaVantageDataProvider>();
            services.AddTransient<IApiDataProvider, YahooFinanceProvider>();
            services.AddSingleton<StrategyProvider>();
            services.AddSingleton<IInvestorProvider, InvestorProvider>();
            services.AddSingleton<MarketDataProvider>();
            services.AddSingleton<ReportProvider>();

            // Factories
            services.AddSingleton<StrategyFactory>();
            services.AddSingleton<SimulatorFactory>();

            // Services
            services.AddSingleton<ICommunicationService, EmailCommunicationService>();
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

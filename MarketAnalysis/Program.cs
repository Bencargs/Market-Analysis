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
using System.IO;
using System.Threading.Tasks;
using MarketAnalysis.Simulation;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;

namespace MarketAnalysis
{
    public class Program
    {
        public static async Task Main(string[] _)
        {
            try
            {
                using var op = Operation.Begin("Performing market analysis");
                RegisterConfiguration();
                RegisterLogger();
                await using var provider = RegisterServices();
                var service = provider.GetService<AnalysisService>();
                await service!.Execute();
                op.Complete();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception occured:");
                throw;
            }
        }

        private static ServiceProvider RegisterServices()
        {
            var services = new ServiceCollection();

            // Caches
            services.AddSingleton<IMarketDataCache, MarketDataCache>();
            services.AddSingleton<ISimulationCache, SimulationCache>();

            // Repositories
            services.AddTransient<IRepository<MarketData>, FileRepository>();
            services.AddTransient<IRepository<SimulationResult>, FileRepository>();

            // Providers
            services.AddTransient<ProgressBarProvider>();
            services.AddSingleton<IResultsProvider, ResultsProvider>();
            //services.AddSingleton<IApiDataProvider, WorldTradingDataProvider>();
            //services.AddSingleton<IApiDataProvider, AlphaVantageDataProvider>();
            services.AddTransient<IApiDataProvider, YahooFinanceProvider>();
            services.AddTransient<StrategyProvider>();
            services.AddSingleton<IInvestorProvider, InvestorProvider>();
            services.AddTransient<MarketDataProvider>();
            services.AddTransient<MarketStatisticsProvider>();
            services.AddTransient<ReportProvider>();

            // Factories
            services.AddTransient<StrategyFactory>();
            services.AddTransient<SimulatorFactory>();

            // Services
            services.AddTransient<ICommunicationService, EmailCommunicationService>();
            services.AddTransient<RatingService>();
            services.AddTransient<AnalysisService>();

            return services.BuildServiceProvider();
        }

        private static void RegisterLogger()
        {
            var logPath = DirectoryManager.GetLatestLogFile();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(logPath)
                .CreateLogger();
        }

        private static void RegisterConfiguration()
        {
            var settingsFile = GetSettingsFile();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(settingsFile, false, true)
                .Build();
            IConfigurationSection settings = configuration.GetSection("Settings");
            Configuration.Initialise(settings);
        }

        private static string GetSettingsFile()
        {
            var platform = "windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                platform = "linux";

            return $"appsettings.{platform}.json";
        }
    }
}

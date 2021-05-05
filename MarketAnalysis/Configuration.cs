using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MarketAnalysis
{
    public static class Configuration
    {
        private static IConfiguration _configuration;

        public static void Initialise(IConfiguration configuration)
            => _configuration = configuration;

        public static string RelativePath => _configuration["RelativePath"];
        public static string LogPath => GetAbsolutePath(_configuration["LogPath"]);
        public static string ReportsPath => GetAbsolutePath(_configuration["ReportsPath"]);
        public static string DataPath => GetAbsolutePath(_configuration["DataPath"]);
        public static string ResultsPath => GetAbsolutePath(_configuration["ResultsPath"]);
        public static string AlphaApiEndpoint => _configuration["AlphaApiEndpoint"];
        public static string AlphaQueryString => _configuration["AlphaQueryString"];
        public static string AlphaApiKey => Environment.GetEnvironmentVariable("AlphaApiKey", EnvironmentVariableTarget.Process);
        public static string WorldApiEndpoint => _configuration["WorldApiEndpoint"];
        public static string WorldQueryString => _configuration["WorldQueryString"];
        public static string WorldApiKey => Environment.GetEnvironmentVariable("WorldApiKey", EnvironmentVariableTarget.Process);
        public static string YahooApiEndpoint => _configuration["YahooApiEndpoint"];
        public static string YahooQueryString => _configuration["YahooQueryString"];
        public static string SmtpApiKey => Environment.GetEnvironmentVariable("SmptApiKey", EnvironmentVariableTarget.Process);
        public static string LogoImagePath => GetAbsolutePath(_configuration["LogoImagePath"]);
        public static string WorldImagePath => GetAbsolutePath(_configuration["WorldImagePath"]);
        public static string PhoneImagePath => GetAbsolutePath(_configuration["PhoneImagePath"]);
        public static string EmailImagePath => GetAbsolutePath(_configuration["EmailImagePath"]);
        public static string EmailTemplatePath => GetAbsolutePath(_configuration["EmailTemplatePath"]);
        public static string StrategyTemplatePath => GetAbsolutePath(_configuration["StrategyTemplatePath"]);
        public static string PatternRecognitionImagePath => GetAbsolutePath(_configuration["PatternRecognitionImagePath"]);
        public static DateTime BacktestingDate => DateTime.Parse(_configuration["BacktestingDate"]);
        
        private static string GetAbsolutePath(string path)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            return Path.Combine(currentDirectory, RelativePath, path);
        }
    }
}

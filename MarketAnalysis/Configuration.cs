using System;
using System.Configuration;
using System.IO;

namespace MarketAnalysis
{
    public static class Configuration
    {
        public static string LogPath => GetAbsolutePath(ConfigurationManager.AppSettings["LogPath"]);
        public static string ReportsPath => GetAbsolutePath(ConfigurationManager.AppSettings["ReportsPath"]);
        public static string DataPath => GetAbsolutePath( ConfigurationManager.AppSettings["DataPath"]);
        public static string ResultsPath => GetAbsolutePath(ConfigurationManager.AppSettings["ResultsPath"]);
        public static string ApiEndpoint => ConfigurationManager.AppSettings["ApiEndpoint"];
        public static string QueryString => ConfigurationManager.AppSettings["QueryString"];
        public static string ApiKey => Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.User);
        public static string SmtpApiKey => Environment.GetEnvironmentVariable("SmptApiKey", EnvironmentVariableTarget.User);
        public static string LogoImagePath => GetAbsolutePath(ConfigurationManager.AppSettings["LogoImagePath"]);
        public static string WorldImagePath => GetAbsolutePath(ConfigurationManager.AppSettings["WorldImagePath"]);
        public static string PhoneImagePath => GetAbsolutePath(ConfigurationManager.AppSettings["PhoneImagePath"]);
        public static string EmailImagePath => GetAbsolutePath(ConfigurationManager.AppSettings["EmailImagePath"]);
        public static string EmailTemplatePath => GetAbsolutePath(ConfigurationManager.AppSettings["EmailTemplatePath"]);
        public static string PatternRecognitionImagePath => GetAbsolutePath(ConfigurationManager.AppSettings["PatternRecognitionImagePath"]);
        public static decimal DailyFunds { get; } = decimal.Parse(ConfigurationManager.AppSettings["DailyFunds"]);
        public static DateTime BacktestingDate { get; } = DateTime.Parse(ConfigurationManager.AppSettings["BacktestingDate"]);
        public static int CacheSize { get; } = int.Parse(ConfigurationManager.AppSettings["CacheSize"]);

        private static string GetAbsolutePath(string relativePath)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            return Path.Combine(currentDirectory, "..\\..\\..\\", relativePath);
        }
    }
}

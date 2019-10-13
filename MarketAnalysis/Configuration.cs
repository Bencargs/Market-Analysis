using System;
using System.Configuration;

namespace MarketAnalysis
{
    public static class Configuration
    {
        public static bool InitialRun => bool.Parse(ConfigurationManager.AppSettings["InitialRun"]);
        public static string LogPath => ConfigurationManager.AppSettings["LogPath"];
        public static string ReadonlyDataPath => ConfigurationManager.AppSettings["ReadonlyDataPath"];
        public static string DataPath => ConfigurationManager.AppSettings["DataPath"];
        public static string ResultsPath => ConfigurationManager.AppSettings["ResultsPath"];
        public static string ApiEndpoint => ConfigurationManager.AppSettings["ApiEndpoint"];
        public static string QueryString => ConfigurationManager.AppSettings["QueryString"];
        public static string ApiKey => Environment.GetEnvironmentVariable("ApiKey", EnvironmentVariableTarget.User);
        public static string SmtpApiKey => Environment.GetEnvironmentVariable("SmptApiKey", EnvironmentVariableTarget.User);
        public static string LogoImagePath => ConfigurationManager.AppSettings["LogoImagePath"];
        public static string WorldImagePath => ConfigurationManager.AppSettings["WorldImagePath"];
        public static string PhoneImagePath => ConfigurationManager.AppSettings["PhoneImagePath"];
        public static string EmailImagePath => ConfigurationManager.AppSettings["EmailImagePath"];
        public static string EmailTemplatePath => ConfigurationManager.AppSettings["EmailTemplatePath"];
        public static string PatternRecognitionImagePath => ConfigurationManager.AppSettings["PatternRecognitionImagePath"];
        public static decimal OnlinePriceAdjustment => decimal.Parse(ConfigurationManager.AppSettings["OnlinePriceAdjustment"]);
        public static int CacheSize => int.Parse(ConfigurationManager.AppSettings["CacheSize"]);
    }
}

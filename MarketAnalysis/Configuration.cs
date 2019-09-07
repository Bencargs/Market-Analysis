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
        public static string SmtpServer => ConfigurationManager.AppSettings["SmtpServer"];
        public static int SmtpPort => int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
        public static string SmtpUsername => Environment.GetEnvironmentVariable("SmtpUsername", EnvironmentVariableTarget.User);
        public static string SmtpPassword => Environment.GetEnvironmentVariable("SmtpPassword", EnvironmentVariableTarget.User);
        public static string EmailTemplatePath => ConfigurationManager.AppSettings["EmailTemplatePath"];
        public static string PatternRecognitionImagePath => ConfigurationManager.AppSettings["PatternRecognitionImagePath"];
        public static decimal OnlinePriceAdjustment => decimal.Parse(ConfigurationManager.AppSettings["OnlinePriceAdjustment"]);
        public static int OptimisePeriod => int.Parse(ConfigurationManager.AppSettings["OptimisePeriod"]);
    }
}

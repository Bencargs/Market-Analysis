namespace MarketAnalysis.Models.Reporting
{
    public class Report
    {
        public string Title { get; set; }

        public ReportPage CoverPage => Contents[0];
        public ReportPage Summary => Contents[1];
        public ReportPage MarketReport => Contents[2];
        public ReportPage[] StrategyReports => Contents[3..];
        public ReportPage[] Contents { get; set; }
    }
}

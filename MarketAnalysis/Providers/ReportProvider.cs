using MarketAnalysis.Models;
using MarketAnalysis.Models.Reporting;
using MarketAnalysis.Reports;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis.Providers
{
    public class ReportProvider
    {
        public async Task<Report> GenerateReports(Investor investor, IEnumerable<SimulationResult> results)
        {
            var date = results.First().Date.ToString("dd MMM yyyy");
            var coverPage = GetCoverPage();
            var summary = await new SummaryReport(investor, results).Build();
            var marketReport = GetMarketReport();
            var strategyReports = await GetStrategyReportsAsync(results);

            // temporary
            var json = JsonConvert.SerializeObject(results);
            var bytes = Encoding.ASCII.GetBytes(json);
            summary.AddJsonFile("reslts.json", bytes);

            return new Report
            {
                Title = $"Market Report {date}",
                Contents = new[]
                {
                    coverPage,
                    summary,
                    marketReport
                }
                .Union(strategyReports)
                .ToArray()
            };
        }

        private ReportPage GetCoverPage()
        {
            return new ReportPage("");
        }

        private ReportPage GetMarketReport()
        {
            return new ReportPage("");
        }

        private async Task<ReportPage[]> GetStrategyReportsAsync(IEnumerable<SimulationResult> results)
        {
            var reports = new List<ReportPage>();
            foreach (var r in results)
            {
                reports.Add(await new StrategyReport(r).Build());
            }
            return reports.ToArray();
        }
    }
}

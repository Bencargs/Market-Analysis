using MarketAnalysis.Models;
using MarketAnalysis.Models.Reporting;
using MarketAnalysis.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis.Reports
{
    public class SummaryReport
    {
        private readonly DateTime _date;
        private readonly Investor _investor;
        private readonly IEnumerable<SimulationResult> _results;

        public SummaryReport(Investor investor, IEnumerable<SimulationResult> results)
        {
            _investor = investor;
            _results = results;
            _date = results.First().Date;
        }

        public async Task<ReportPage> Build()
        {
            var path = Configuration.EmailTemplatePath;
            var content = await File.ReadAllTextAsync(path);

            var report = new ReportPage(content);
            AddBody(report);
            AddResultsSummary(report);
            AddImages(report);

            return report;
        }

        private void AddBody(ReportPage template)
        {
            template.Replace("date", _date.ToString("dd/MM/yyyy"));
            template.Replace("InvestorName", _investor.Name);
            template.Replace("InvestorNumber", _investor.Number);
            template.Replace("recommendation", template.GetRecommendation(ResultsProvider.ShouldBuy(_results)));
            template.Replace("profit", ResultsProvider.TotalProfit(_results).ToString("C2"));
        }

        private void AddResultsSummary(ReportPage template)
        {
            var summary = new StringBuilder();
            foreach (var s in _results)
            {
                summary.Append("<tr>");
                summary.Append($"<td>{s.StrategyType}</td>");
                summary.Append($"<td>{template.GetRecommendation(s.ShouldBuy)}</td>");
                summary.Append($"<td>{s.ProfitYTD.ToString("C2")}</td>");
                summary.Append($"<td>{s.ProfitTotal.ToString("C2")}</td>");
                summary.Append($"<td>{s.BuyCount}</td>");
                summary.Append("</tr>");
            }
            template.Replace("results", summary.ToString());
        }

        private void AddImages(ReportPage template)
        {
            template.AddImage("logo", Configuration.LogoImagePath);
            template.AddImage("website", Configuration.WorldImagePath);
            template.AddImage("phone", Configuration.PhoneImagePath);
            template.AddImage("email", Configuration.EmailImagePath);
        }
    }
}

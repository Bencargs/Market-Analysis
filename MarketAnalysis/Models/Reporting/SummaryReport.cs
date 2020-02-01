using MarketAnalysis.Models;
using MarketAnalysis.Models.Reporting;
using MarketAnalysis.Providers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarketAnalysis.Reports
{
    public class SummaryReport : ReportPage
    {
        public SummaryReport(RecipientDetails recipient, IResultsProvider resultsProvider)
        {
            AddTemplate();
            AddBody(recipient, resultsProvider.TotalProfit(), resultsProvider.ShouldBuy());
            AddResultsSummary(resultsProvider.GetResults());
            AddImages();
        }

        private void AddTemplate()
        {
            var path = Configuration.EmailTemplatePath;
            var content = File.ReadAllText(path);
            Body = content;
        }

        private void AddBody(RecipientDetails recipient, decimal totalProfit, bool shouldBuy)
        {
            Replace("date", recipient.Date.ToString("dd/MM/yyyy"));
            Replace("InvestorName", recipient.Name);
            Replace("InvestorNumber", recipient.Number);
            Replace("recommendation", ToRecommendation(shouldBuy));
            Replace("profit", totalProfit.ToString("C2"));
        }

        private string AddResultsSummary(IEnumerable<SimulationResult> results)
        {
            var summary = new StringBuilder();
            foreach (var s in results)
            {
                summary.Append("<tr>");
                summary.Append($"<td>{s.StrategyName}</td>");
                summary.Append($"<td>{ToRecommendation(s.ShouldBuy)}</td>");
                summary.Append($"<td>{s.ProfitYTD.ToString("C2")}</td>");
                summary.Append($"<td>{s.ProfitTotal.ToString("C2")}</td>");
                summary.Append($"<td>{s.BuyCount}</td>");
                summary.Append("</tr>");
            }
            return summary.ToString();
        }

        public void AddImages()
        {
            AddImage("logo", Configuration.LogoImagePath);
            AddImage("website", Configuration.WorldImagePath);
            AddImage("phone", Configuration.PhoneImagePath);
            AddImage("email", Configuration.EmailImagePath);
        }

        private static string ToRecommendation(bool shouldBuy) => shouldBuy ? "Buy" : "Hold";
    }
}

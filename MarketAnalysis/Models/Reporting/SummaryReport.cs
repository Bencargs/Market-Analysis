using MarketAnalysis.Models.Reporting;
using MarketAnalysis.Providers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis.Reports
{
    public class SummaryReport
    {
        private RecipientDetails _recipient;
        private IResultsProvider _resultsProvider;

        public SummaryReport(RecipientDetails recipient, IResultsProvider resultsProvider)
        {
            _recipient = recipient;
            _resultsProvider = resultsProvider;
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
            template.Replace("date", _recipient.Date.ToString("dd/MM/yyyy"));
            template.Replace("InvestorName", _recipient.Name);
            template.Replace("InvestorNumber", _recipient.Number);
            template.Replace("recommendation", ToRecommendation(_resultsProvider.ShouldBuy()));
            template.Replace("profit", _resultsProvider.TotalProfit().ToString("C2"));
        }

        private void AddResultsSummary(ReportPage template)
        {
            var summary = new StringBuilder();
            foreach (var s in _resultsProvider.GetResults())
            {
                summary.Append("<tr>");
                summary.Append($"<td>{s.StrategyName}</td>");
                summary.Append($"<td>{ToRecommendation(s.ShouldBuy)}</td>");
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

        private static string ToRecommendation(bool shouldBuy) => shouldBuy ? "Buy" : "Hold";
    }
}

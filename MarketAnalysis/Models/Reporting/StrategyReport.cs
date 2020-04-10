using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Models.Reporting
{
    public class StrategyReport
    {
        private readonly SimulationResult _results;

        public StrategyReport(SimulationResult results)
        {
            _results = results;
        }

        public async Task<ReportPage> Build()
        {
            var path = Configuration.StrategyTemplatePath;
            var content = await File.ReadAllTextAsync(path);

            var report = new ReportPage(content);
            AddBody(report);
            AddHeader(report);
            AddCharts(report);

            return report;
        }

        private void AddBody(ReportPage template)
        {
            template.Replace("strategy", _results.StrategyType);
            template.Replace("shouldBuy", template.GetRecommendation(_results.ShouldBuy));
            
            template.Replace("profitTotal", $"{_results.ProfitTotal:C2}");
            template.Replace("profitYTD", $"{_results.ProfitYTD:C2}");
            template.Replace("aboveMarket", $"{_results.AboveMarketReturn:C2}");
            
            template.Replace("alpha", $"{_results.Alpha:P2}");
            template.Replace("maximumAlpha", $"{_results.MaximumAlpha:P2}");

            template.Replace("buyCount", $"{_results.BuyCount}");
            template.Replace("drawdown", $"{_results.MaximumDrawdown:C2}");
            template.Replace("holdingPeriod", $"{_results.MaximumHoldingPeriod}");

            template.Replace("sharpeRatio", $"{_results.SharpeRatio:0.00}");
            template.Replace("correlation", $"{_results.MarketCorrelation:P2}");
            template.Replace("averageReturn", $"{_results.AverageReturn:C2}");

            template.Replace("accuracy", $"{_results.Accuracy:P2}");
            template.Replace("precision", $"{_results.Precision:P2}");
            template.Replace("recall", $"{_results.Recall:P2}");

            template.Replace("generalDescription", PlaceholderText);
        }

        private void AddHeader(ReportPage template)
        {
            template.AddImage("logo", Configuration.LogoImagePath, true);
            template.AddImage("website", Configuration.WorldImagePath, true);
            template.AddImage("phone", Configuration.PhoneImagePath, true);
            template.AddImage("email", Configuration.EmailImagePath, true);
        }

        private void AddCharts(ReportPage template)
        {
            var returnsChart = new Chart("Strategy returns", "Return ($ AU)", "Time (Days)")
                .AddSeries(_results.MarketAverage, "Market Average")
                .AddSeries(_results.History, _results.StrategyType);
            template.AddChart("image1", returnsChart);

            var relative = _results.MarketAverage.Select((x, i) => (_results.History[i] - x));
            var profitLossChart = new Chart("Performance vs Market Average", "Profit/Loss ($ AU)", "Time (Days)")
                .AddSeries(relative, _results.StrategyType);
            template.AddChart("image2", profitLossChart);
        }

        private const string PlaceholderText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    }
}

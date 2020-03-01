using System.IO;
using System.Threading.Tasks;

namespace MarketAnalysis.Models.Reporting
{
    public class StrategyReport
    {
        private SimulationResult _strategy;

        public StrategyReport(SimulationResult strategy)
        {
            _strategy = strategy;
        }

        public async Task<ReportPage> Build()
        {
            var path = Configuration.StrategyTemplatePath;
            var content = await File.ReadAllTextAsync(path);

            var report = new ReportPage(content);
            AddBody(report);
            AddImages(report);

            return report;
        }

        private void AddBody(ReportPage template)
        {
            template.Replace("strategy", _strategy.StrategyType);
            template.Replace("shouldBuy", template.GetRecommendation(_strategy.ShouldBuy));
            
            template.Replace("profitTotal", $"{_strategy.ProfitTotal:C2}");
            template.Replace("profitYTD", $"{_strategy.ProfitYTD:C2}");
            template.Replace("aboveMarket", $"{_strategy.AboveMarketReturn:C2}");
            
            template.Replace("alpha", $"{_strategy.Alpha:P2}");
            template.Replace("maximumAlpha", $"{_strategy.MaximumAlpha:P2}");

            template.Replace("buyCount", $"{_strategy.BuyCount}");
            template.Replace("drawdown", $"{_strategy.MaximumDrawdown:C2}");
            template.Replace("holdingPeriod", $"{_strategy.MaximumHoldingPeriod}");

            template.Replace("sharpeRatio", $"{_strategy.SharpeRatio:P2}");
            template.Replace("correlation", $"{_strategy.MarketCorrelation:P2}");
            template.Replace("averageReturn", $"{_strategy.AverageReturn:C2}");

            template.Replace("accuracy", $"{_strategy.Accuracy:P2}");
            template.Replace("precision", $"{_strategy.Precision:P2}");
            template.Replace("recall", $"{_strategy.Recall:P2}");

            template.Replace("generalDescription", PlaceholderText);
        }

        private void AddImages(ReportPage template)
        {
            template.AddImage("image1", Configuration.LogoImagePath);
            template.AddImage("image2", Configuration.LogoImagePath);
            template.AddImage("image3", Configuration.LogoImagePath);
        }

        private const string PlaceholderText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    }
}

using MarketAnalysis.Services;
using System.ServiceProcess;

namespace MarketAnalysis
{
    internal class WindowsService : ServiceBase
    {
        private readonly AnalysisService _analysisService;

        public WindowsService(AnalysisService analysisService)
        {
            ServiceName = "Market Analysis Service";
            _analysisService = analysisService;
        }

        protected override void OnStart(string[] args)
        {
            _analysisService.Execute().Wait();
        }

        protected override void OnStop()
        {
        }
    }
}

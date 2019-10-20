using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MarketAnalysisTests
{
    public class EmailCommunicationServiceTests
    {
        [Fact(Skip = "This test is unsafe, run only as required")]
        public async Task SendCommunicationTest()
        {
            var resultsProvider = new Mock<IResultsProvider>();
            resultsProvider.Setup(_ => _.GetResults()).Returns(new SimulationResult[0]);
            var emailProvider = new Mock<IProvider>();
            var service = new EmailCommunicationService(emailProvider.Object);

            await service.SendCommunication(resultsProvider.Object);
        }
    }
}

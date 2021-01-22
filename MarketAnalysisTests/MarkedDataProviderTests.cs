using System;
using System.Linq;
using System.Threading.Tasks;
using ExpectedObjects;
using MarketAnalysis.Models;
using MarketAnalysis.Providers;
using MarketAnalysis.Repositories;
using Moq;
using NUnit.Framework;

namespace MarketAnalysisTests
{
    public class MarkedDataProviderTests
    {
        [Test]
        public async Task MarketDataProvider_MergesHistoricAndLiveData()
        {
            var dataRepository = new Mock<IRepository<MarketData>>();
            dataRepository
                .Setup(x => x.Get())
                .ReturnsAsync(new[]
                {
                    new MarketData {Date = new DateTime(2000,1,1)},
                    new MarketData {Date = new DateTime(2000,1,3)},
                });
            var apiDataProvider = new Mock<IApiDataProvider>();
            apiDataProvider
                .Setup(x => x.GetData())
                .ReturnsAsync(new[]
                {
                    new MarketData {Date = new DateTime(2000,1,2)},
                    new MarketData {Date = new DateTime(2000,1,4)},
                });
            var target = new MarketDataProvider(
                apiDataProvider.Object,
                dataRepository.Object);

            var actual = await target.GetPriceData();

            CollectionAssert.AreEqual(
                new[] { 1, 2, 3, 4 },
                actual.Select(x => x.Date.Day));
        }

        [Test]
        public async Task MarketDataProvider_DeltaCorrectAtJoinPoint()
        {
            var dataRepository = new Mock<IRepository<MarketData>>();
            dataRepository
                .Setup(x => x.Get())
                .ReturnsAsync(new[]
                {
                    new MarketData {Date = new DateTime(2000,1,1), Price = 1, Volume = 1},
                    new MarketData {Date = new DateTime(2000,1,2), Price = 1, Delta = 2, Volume = 2},
                });

            var apiDataProvider = new Mock<IApiDataProvider>();
            apiDataProvider
                .Setup(x => x.GetData())
                .ReturnsAsync(new[]
                {
                    new MarketData {Date = new DateTime(2000,1,3), Price = 5, Volume = 5},
                });
            var target = new MarketDataProvider(
                apiDataProvider.Object,
                dataRepository.Object);

            var actual = await target.GetPriceData();

            var last = actual.Last();
            new
            {
                Delta = 4m,
                DeltaPercent = -0.5m,
                VolumePercent = -0.6m
            }.ToExpectedObject().ShouldEqual(new
            {
                last.Delta,
                last.DeltaPercent,
                last.VolumePercent
            });
        }
    }
}

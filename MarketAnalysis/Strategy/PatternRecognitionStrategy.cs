using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class PatternRecognitionStrategy : OptimisableStrategy
    {
        private Image _average;
        private double _threshold;
        public override StrategyType StrategyType { get; } = StrategyType.PatternRecognition;
        protected override TimeSpan OptimisePeriod => TimeSpan.FromDays(1024);

        public PatternRecognitionStrategy(double threshold, Image average = null, bool shouldOptimise = true)
            : base(shouldOptimise)
        {
            _threshold = threshold;
            _average = average ?? new Func<Image>(() =>
            {
                var averagePath = Configuration.PatternRecognitionImagePath;
                return new Image(averagePath);
            }).Invoke();
        }

        public override IEnumerable<IStrategy> GetOptimisations()
        {
            var history = MarketDataCache.Instance.TakeUntil(LatestDate).Select(x => x.Price).ToArray();
            var training = history.Batch(30).Select(b =>
            {
                var batch = b.ToArray();
                var min = batch.Min();
                var minIndex = Array.IndexOf(batch, min);
                var range = new ArraySegment<decimal>(history, minIndex, 30).ToList();
                return CreateImage(range, 30, 30);
            }).ToList();
            if (training.Any())
                _average = CreateAverage(training);

            return Enumerable.Range(600, 300).Select(x =>
                new PatternRecognitionStrategy(x, _average, false));

            // this approach is ideal, but prohibitively slow
            //var potentials = Enumerable.Range(0, _average.Width).SelectMany(x =>
            //{
            //    return Enumerable.Range(0, _average.Height).SelectMany(y =>
            //    {
            //        return Enumerable.Range(0, 25).Select(i =>
            //        {
            //            var candidate = new Image(_average);
            //            candidate.SetPixel(x, y, i * 10);

            //            return new PatternRecognitionStrategy(_threshold, candidate, false);
            //        });
            //    });
            //});
        }

        public override void SetParameters(IStrategy strategy)
        {
            var optimal = (PatternRecognitionStrategy)strategy;
            _threshold = optimal._threshold;
            _average = optimal._average;
        }

        private Image CreateAverage(List<Image> images)
        {
            if (!images.Any())
                return null;

            var xSize = images.Max(x => x.Width);
            var ySize = images.Max(x => x.Height);

            var average = new Image(xSize, ySize);
            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                {
                    var sum = images.Sum(i => i.GetPixel(x, y));
                    var value = sum / images.Count();
                    average.SetPixel(x, y, value);
                }
            average.ComputeHash();
            return average;
        }

        protected override bool ShouldBuy(MarketData data)
        {
            var dataset = MarketDataCache.Instance.GetLastSince(LatestDate, 30)
                .Select(x => x.Price).ToList();
            if (dataset.Count < 30)
                return false;

            var image = CreateImage(dataset, 30, 30);
            if (image == null)
                return false;

            var value = EvaluateImage(_average, image);
            return value > _threshold;
        }

        private Image CreateImage(List<decimal> dataset, int xSize, int ySize)
        {
            var yMax = dataset.Max();
            var yMin = dataset.Min();
            int yRange = Convert.ToInt32(yMax - yMin);
            if (yRange == 0)
                return null;

            var image = new Image(xSize, ySize);
            for (int x = 0; x < dataset.Count; x++)
            {
                var price = (int)dataset[x] - yMin;
                var yPos = (int)((price / yRange) * ySize);

                for (int y = 0; y < ySize; y++)
                {
                    if (y == yPos)
                        image.SetPixel(x, y, 0);
                    else
                        image.SetPixel(x, y, 255);
                }
            }
            image.ComputeHash();
            return image;
        }

        private double EvaluateImage(Image average, Image testImage)
        {
            if (average == null || testImage == null)
                return 0;

            var sum = 0;
            var total = 0;
            for (int x = 0; x < testImage.Width; x++)
            {
                var min = 255;
                for (int y = 0; y < testImage.Height; y++)
                {
                    var pixel = average.GetPixel(x, y);

                    if (testImage.GetPixel(x, y) != 255)
                        sum += (255 - pixel);

                    if (pixel < min)
                        min = pixel;
                }
                total += (255 - min);
            }
            return sum;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PatternRecognitionStrategy strategy))
                return false;

            return strategy._threshold == _threshold && 
                   _average.Equals(strategy._average);
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode() ^ 
                   _average.GetHashCode();
        }
    }
}

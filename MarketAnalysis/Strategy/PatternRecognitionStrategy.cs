using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class PatternRecognitionStrategy : IStrategy
    {
        private Image _average;
        private double _threshold;
        private static TimeSpan OptimisePeriod = TimeSpan.FromDays(1024);
        private DateTime _latestDate;
        private DateTime? _lastOptimised;

        public object Key => _average;

        public PatternRecognitionStrategy(double threshold, Image average = null, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _lastOptimised = shouldOptimise ? DateTime.MinValue : (DateTime?) null;
            _average = average ?? new Func<Image>(() =>
            {
                var averagePath = Configuration.PatternRecognitionImagePath;
                return new Image(averagePath);
            }).Invoke();
        }

        public bool ShouldOptimise()
        {
            if (_lastOptimised != null &&
                _latestDate > (_lastOptimised + OptimisePeriod))
            {
                _lastOptimised = _latestDate;
                return true;
            }
            return false;
        }

        public IEnumerable<IStrategy> GetOptimisations()
        {
            //var history = MarketDataCache.Instance.TakeUntil(_latestDate).ToList();
            //using (var progress = ProgressBarReporter.SpawnChild(history.Count / OptimisePeriod, "Optimising..."))
            //{
            //    var minimums = new List<Image>();
            //    for (int i = OptimisePeriod; i < history.Count - OptimisePeriod; i += OptimisePeriod)
            //    {
            //        var batch = history.GetRange(i, OptimisePeriod);
            //        var minBatchIndex = batch.Select(x => x.Price).ToList().IndexOfMin();
            //        var minRangeIndex = (i - OptimisePeriod) + minBatchIndex;
            //        var dataset = history.GetRange(minRangeIndex, OptimisePeriod)
            //            .Select(x => x.Price)
            //            .ToList();
            //        var image = CreateImage(dataset, OptimisePeriod, OptimisePeriod);
            //        minimums.Add(image);
            //        progress.Tick($"Optimising... x: {i}");
            //    }
            //    if (minimums.Any())
            //        _average = CreateAverage(minimums);
            //}

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

        public void SetParameters(IStrategy strategy)
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

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(MarketData data)
        {
            if (data.Date > _latestDate)
                _latestDate = data.Date;

            var history = MarketDataCache.Instance.TakeUntil(_latestDate).ToList();
            if (history.Count < 30)
                return false;

            var dataset = history.GetRange(history.Count - 30, 30)
                .Select(x => x.Price).ToList();

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
                        image.SetPixel(x, y, 255);
                    else
                        image.SetPixel(x, y, 0);
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
                        sum += 255 - pixel;

                    if (pixel < min)
                        min = pixel;
                }
                total += (255 - min);
            }
            return sum;
        }

        public override bool Equals(object obj)
        {
            var strategy = (obj as PatternRecognitionStrategy);
            var key = strategy?.Key;
            var threshold = strategy?._threshold;
            if (key == null || threshold == null)
                return false;

            return threshold == _threshold && Key.Equals(key);
        }

        public override int GetHashCode()
        {
            return _threshold.GetHashCode() ^ Key.GetHashCode();
        }
    }
}

﻿using MarketAnalysis.Caching;
using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class PatternRecognitionStrategy : IStrategy
    {
        private double _threshold;
        private Image _average;
        private bool _shouldOptimise;
        private const int OptimisePeriod = 30;
        private DateTime _latestDate;

        public object Key => _average;

        public PatternRecognitionStrategy(double threshold, Image average = null, bool shouldOptimise = true)
        {
            _threshold = threshold;
            _shouldOptimise = shouldOptimise;
            _average = average ?? new Func<Image>(() =>
                 {
                     var averagePath = Configuration.PatternRecognitionImagePath;
                     return new Image(averagePath);
                 }).Invoke();
        }

        public bool ShouldOptimise()
        {
            var count = MarketDataCache.Instance.Count;
            return _shouldOptimise &&
                   count > 1 &&
                   count % OptimisePeriod == 0;
        }

        public void Optimise()
        {
            var history = MarketDataCache.Instance.TakeUntil(_latestDate).ToList();
            using (var progress = ProgressBarReporter.SpawnChild(history.Count / OptimisePeriod, "Optimising..."))
            {
                var minimums = new List<Image>();
                for (int i = OptimisePeriod; i < history.Count - OptimisePeriod; i += OptimisePeriod)
                {
                    var batch = history.GetRange(i, OptimisePeriod);
                    var minBatchIndex = batch.Select(x => x.Price).ToList().IndexOfMin();
                    var minRangeIndex = (i - OptimisePeriod) + minBatchIndex;
                    var dataset = history.GetRange(minRangeIndex, OptimisePeriod)
                        .Select(x => x.Price)
                        .ToList();
                    var image = CreateImage(dataset, OptimisePeriod, OptimisePeriod);
                    minimums.Add(image);
                    progress.Tick($"Optimising... x: {i}");
                }
                if (minimums.Any())
                    _average = CreateAverage(minimums);
            }

            // this approach is ideal, but prohibitively slow
            //using (var progress = ProgressBarReporter.SpawnChild(_average.Width * _average.Height, "Optimising..."))
            //{
            //    var simulator = new Simulator(_history);
            //    var clone = new Image(_average);
            //    var width = _average.Width - Math.Min(OptimisePeriod, _average.Width - 1);
            //    for (int x = width; x < _average.Width; x++)
            //    {
            //        for (int y = 0; y < _average.Height; y++)
            //        {
            //            var optimal = Enumerable.Range(0, 255).Select(i =>
            //            {
            //                clone.SetPixel(x, y, i);
            //                var result = simulator.Evaluate(new PatternRecognitionStrategy(_threshold, clone, false)).Last();
            //                return new { i, result.Worth, result.BuyCount };
            //            }).OrderByDescending(v => v.Worth).ThenBy(v => v.BuyCount).First();

            //            clone.SetPixel(x, y, optimal.i);
            //            progress.Tick($"Optimising... x: {x} y: {y}");
            //        }
            //    }
            //    _average = clone;
            //}
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
            if (MarketDataCache.Instance.TryAdd(data))
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
            var key = (obj as PatternRecognitionStrategy)?.Key;
            if (key == null)
                return false;
                 
            return Key.Equals(key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}

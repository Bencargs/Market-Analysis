using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MarketAnalysis.Strategy
{
    public class PatternRecognitionStrategy : IStrategy
    {
        private double _threshold;
        private Bitmap _average;
        private List<Row> _history = new List<Row>(5000);

        public PatternRecognitionStrategy(double threshold, Bitmap average = null)
        {
            _threshold = threshold;
            _average = average ?? new Func<Bitmap>(() =>
                 {
                     var averagePath = Configuration.PatternRecognitionImagePath;
                     return new Bitmap(averagePath);
                 }).Invoke();
        }

        public void Optimise()
        {
            //Disabled untill I have an idea on how long this takes to complete
            return;

            var simulator = new Simulation(_history, false);
            using (var clone = new Bitmap(_average))
            {
                var width = _average.Width - Math.Min(Configuration.OptimisePeriod, _average.Width - 1);
                for (int x = width; x < _average.Width; x++)
                    for (int y = 0; y < _average.Height; y++)
                    {
                        var optimal = Enumerable.Range(0, 255).Select(i =>
                        {
                            var intensity = Color.FromArgb(i, i, i);
                            clone.SetPixel(x, y, intensity);
                            var result = simulator.Evaluate(new PatternRecognitionStrategy(_threshold, clone));
                            return new { intensity, result.Worth, result.BuyCount };
                        }).OrderByDescending(v => v.Worth).ThenBy(v => v.BuyCount).First();
                        
                        clone.SetPixel(x, y, optimal.intensity);
                    }
                _average = new Bitmap(clone);
            }
        }

        private Bitmap CreateAverage(List<Bitmap> images)
        {
            if (!images.Any())
                return null;

            var xSize = images.Max(x => x.Width);
            var ySize = images.Max(x => x.Height);

            var average = new Bitmap(xSize, ySize);
            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                {
                    var sum = images.Sum(i => i.GetPixel(x, y).R);
                    var value = sum / images.Count();
                    average.SetPixel(x, y, Color.FromArgb(value, value, value));
                }
            return average;
        }

        public bool ShouldAddFunds()
        {
            return true;
        }

        public bool ShouldBuyShares(Row data)
        {
            if (!_history.Any(x => x.Date == data.Date))
                _history.Add(data);

            if (_history.Count < 30)
                return false;

            var dataset = _history.GetRange(_history.Count - 30, 30)
                .Select(x => x.Price).ToList();

            var image = CreateImage(dataset, 30, 30);
            if (image == null)
                return false;

            var value = EvaluateImage(_average, image);
            return value > _threshold;
        }

        private Bitmap CreateImage(List<decimal> dataset, int xSize, int ySize)
        {
            var yMax = dataset.Max();
            var yMin = dataset.Min();
            int yRange = Convert.ToInt32(yMax - yMin);
            if (yRange == 0)
                return null;

            var image = new Bitmap(xSize, ySize);
            for (int x = 0; x < dataset.Count; x++)
            {
                var price = (int)dataset[x] - yMin;
                var yPos = (int)((price / yRange) * ySize);

                for (int y = 0; y < ySize; y++)
                {
                    if (y == yPos)
                        image.SetPixel(x, y, Color.Black);
                    else
                        image.SetPixel(x, y, Color.White);
                }
            }
            return image;
        }

        private double EvaluateImage(Bitmap average, Bitmap testImage)
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
                    var pixel = average.GetPixel(x, y).R;
                    if (testImage.GetPixel(x, y).R != 255)
                        sum += 255 - pixel;

                    if (pixel < min)
                        min = pixel;
                }
                total += (255 - min);
            }
            return sum;
        }
    }
}

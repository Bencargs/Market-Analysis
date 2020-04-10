using MarketAnalysis.Models;
using MarketAnalysis.Simulation;
using MarketAnalysis.Strategy;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Search
{
	public class BinarySearch : ISearcher
	{
		private readonly Image _image;
        private readonly double _threshold;
        private readonly ISimulator _simulator;
        private readonly IProgressBar _progress;

        public BinarySearch(ISimulator simulator, Image image, double threshold, IProgressBar progress)
        {
            _image = image;
            _progress = progress;
            _threshold = threshold;
            _simulator = simulator;
        }

        public IStrategy Maximum(DateTime endDate)
		{
			for (int x = 0; x < _image.Width; x++)
			{
				for (int y = 0; y < _image.Height; y++)
				{
                    var value = ProcessValue(x, y, endDate);
                    _progress?.Tick();
                    _image.SetPixel(x, y, value);
                }
			}
            return new PatternRecognitionStrategy(_threshold, _image, false);
		}

        private int ProcessValue(int x, int y, DateTime endDate)
        {
            var min = 128;
            var max = 256;
            var range = max - min;
            var lookup = new Dictionary<int, decimal>();

            while (range > 1)
            {
                var leftValue = GetOrCreate(_image, x, y, min, endDate, lookup);
                var rightValue = GetOrCreate(_image, x, y, max, endDate, lookup);

                if (leftValue == rightValue)
                {
                    // Performance oriented escape hatch
                    // Small odds of meaningfull performance imporvement if both sides are exactly equal
                    return _image.GetPixel(x, y);
                }

                range /= 2;
                if (leftValue > rightValue)
                {
                    max = min;
                    min -= range;
                }
                else
                {
                    min = max - range;
                }
            }
            return min;
        }

        private decimal GetOrCreate(Image image, int x, int y, int z, DateTime endDate, Dictionary<int, decimal> lookup)
        {
            if (!lookup.TryGetValue(z, out var value))
            {
                image.SetPixel(x, y, z);
                var left = new PatternRecognitionStrategy(_threshold, image, false);
                value = _simulator.Evaluate(left, endDate).Last().Worth;
                lookup[z] = value;
            }
            return value;
        }
	}
}
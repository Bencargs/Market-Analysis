using MarketAnalysis.Caching;
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
        private readonly MarketDataCache _marketDataCache;
        private readonly ISimulator _simulator;
        private readonly IProgressBar _progress;

        public BinarySearch(
            MarketDataCache marketDataCache,
            ISimulator simulator, 
            Image image, 
            double threshold, 
            IProgressBar progress)
        {
            _image = image;
            _progress = progress;
            _threshold = threshold;
            _simulator = simulator;
            _marketDataCache = marketDataCache;
        }

        public IStrategy Maximum(DateTime endDate)
		{
            var lookup = new Dictionary<int, (decimal Value, IStrategy Strategy)>();
			for (int x = 0; x < _image.Width; x++)
			{
				for (int y = 0; y < _image.Height; y++)
				{
                    var value = ProcessValue(x, y, endDate, lookup);
                    _progress?.Tick();
                    _image.SetPixel(x, y, value);
                }
			}
            var optimal = new PatternRecognitionStrategy(_marketDataCache, _threshold, _image, false);
            
            ClearCache(lookup.Values.Select(x => x.Strategy), optimal);
            return optimal;
		}

        private int ProcessValue(int x, int y, DateTime endDate, Dictionary<int, (decimal, IStrategy)> lookup)
        {
            var min = 128;
            var max = 256;
            var range = max - min;

            while (range > 1)
            {
                var leftValue = GetOrCreate(_image, x, y, min, endDate, lookup);
                var rightValue = GetOrCreate(_image, x, y, max, endDate, lookup);

                if (leftValue == rightValue)
                {
                    // Performance escape hatch
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

        private decimal GetOrCreate(Image image, int x, int y, int z, DateTime endDate, Dictionary<int, (decimal Intensity, IStrategy Strategy)> lookup)
        {
            if (!lookup.TryGetValue(z, out var value))
            {
                image.SetPixel(x, y, z);
                var strategy = new PatternRecognitionStrategy(_marketDataCache, _threshold, image, false);
                var intensity = _simulator.Evaluate(strategy, endDate).Last().Worth;
                value = (intensity, strategy);
                lookup[z] = value;
            }
            return value.Intensity;
        }

        private void ClearCache(IEnumerable<IStrategy> potentials, IStrategy optimal)
        {
            _simulator.RemoveCache(potentials.Except(new[] { optimal }));
        }
    }
}
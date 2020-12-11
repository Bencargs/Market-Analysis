//using MarketAnalysis.Caching;
//using MarketAnalysis.Models;
//using MarketAnalysis.Simulation;
//using MarketAnalysis.Strategy;
//using ShellProgressBar;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace MarketAnalysis.Search
//{
//    public class PartitionSearch : ISearcher
//    {
//        private Image _image;
//        private int _partitions;
//        private double _threshold;
//        private ISimulator _simulator;
//        private IProgressBar _progress;
//        private readonly MarketDataCache _marketDataCache;

//        public PartitionSearch(
//            MarketDataCache marketDataCache,
//            ISimulator simulator, 
//            Image image, 
//            double threshold, 
//            int partitions, 
//            IProgressBar progress)
//        {
//            _image = image;
//            _progress = progress;
//            _marketDataCache = marketDataCache;
//            _simulator = simulator;
//            _threshold = threshold;
//            _partitions = partitions;
//        }

//        public IStrategy Maximum(DateTime endDate)
//        {
//            _progress.MaxTicks = (int)Math.Pow(_partitions, 3);
//            var potentials = Enumerable.Range(0, _image.Height / _partitions).SelectMany(y =>
//            {
//                return Enumerable.Range(0, _image.Width / _partitions).SelectMany(x =>
//                {
//                    return Enumerable.Range(0, 255 / _partitions).Select(v =>
//                    {
//                        var candidate = new Image(_image);
//                        candidate.SetPixel(x * _partitions, y * _partitions, v * _partitions);
//                        return new PatternRecognitionStrategy(_marketDataCache, _threshold, candidate, false);
//                    });
//                });
//            });

//            var searcher = new LinearSearch(_simulator, potentials, _progress);
//            var optimal = searcher.Maximum(endDate);

//            ClearCache(potentials, optimal);

//            return optimal;
//        }

//        private void ClearCache(IEnumerable<IStrategy> potentials, IStrategy optimal)
//        {
//            _simulator.RemoveCache(potentials.Except(new[] { optimal }));
//        }
//    }
//}

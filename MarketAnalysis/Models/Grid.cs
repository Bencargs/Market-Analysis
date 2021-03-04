using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Models
{
    public class Grid<T> : IEnumerable<T>
        where T : new()
    {
        private readonly Cell[][] _grid;

        private class Cell
        {
            public Range X { get; init; }
            public Range Y { get; init; }
            public T Value { get; init; }
        }

        public Grid() => _grid = Array.Empty<Cell[]>();

        public Grid(decimal[] xData, decimal[] yData, int partitions)
        {
            _grid = Partition(xData, partitions)
                .Select(x => Partition(yData, partitions)
                    .Select(y => new Cell
                    {
                        X = x,
                        Y = y,
                        Value = new T()
                    }).ToArray()
                ).ToArray();
        }

        public IEnumerator<T> GetEnumerator()
            => _grid.SelectMany(x => x)
                .Select(y => y.Value)
                .GetEnumerator();

        public T this[decimal xValue, decimal yValue]
        {
            get
            {
                var xIndex = GetXIndex(xValue);
                var yIndex = GetYIndex(yValue);

                return _grid[xIndex][yIndex].Value;
            }
        }

        private int GetXIndex(decimal value)
        {
            var length = _grid.GetLength(0) - 1;

            if (value < _grid[0][0].X.Start)
                return 0;
            if (value > _grid[length][0].X.End)
                return length;

            foreach (var xIndex in Enumerable.Range(0, length + 1))
            {
                if (_grid[xIndex][0].X.Contains(value))
                    return xIndex;
            }

            throw new Exception($"Unable to find index of X element {value}");
        }

        private int GetYIndex(decimal value)
        {
            var length = _grid[0].GetLength(0) - 1;
            
            if (value < _grid[0][0].Y.Start)
                return 0;
            if (value > _grid[0][length].Y.End)
                return length;

            foreach (var yIndex in Enumerable.Range(0, length + 1))
            {
                if (_grid[0][yIndex].Y.Contains(value))
                    return yIndex;
            }

            throw new Exception($"Unable to find index of Y element {value}");
        }

        private static Range[] Partition(IEnumerable<decimal> source, int partitions)
        {
            var orderedSource = source.OrderBy(x => x).Distinct().ToArray();
            var population = orderedSource.Length;
            var partitionSize = (int)Math.Ceiling((double)population / partitions);

            var bucket = new List<Range>(partitions);
            foreach (var batch in orderedSource.Batch(partitionSize))
            {
                var range = new Range(batch);

                var index = Array.IndexOf(orderedSource, batch.Last());
                if (index < orderedSource.Length - 1)
                {
                    var end = orderedSource[index + 1];
                    range.End = end;
                }

                bucket.Add(range);
            }
            return bucket.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}

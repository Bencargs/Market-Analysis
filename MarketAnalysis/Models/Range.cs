using System.Collections.Generic;
using System.Linq;

namespace MarketAnalysis.Models
{
    public struct Range
    {
        public decimal Start { get; set; }
        public decimal End { get; set; }

        public Range(decimal start, decimal end)
        {
            Start = start;
            End = end;
        }

        public Range(IEnumerable<decimal> source)
            : this(source.First(), source.Last())
        {
        }

        public bool Contains(decimal value)
        {
            return Start <= value && value <= End;
        }
    }
}

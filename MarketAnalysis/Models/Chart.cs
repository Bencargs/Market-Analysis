using OxyPlot;
using OxyPlot.Core.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarketAnalysis.Models
{
    public class Chart
    {
        private PlotModel _plot;
        private static OxyColor[] _colours = new[]
        {
            OxyColor.FromArgb(255, 149, 196, 235),
            OxyColor.FromArgb(255, 204, 133, 212),
            OxyColor.FromArgb(255, 102, 195, 144),
            OxyColor.FromArgb(255, 243, 155, 155),
            OxyColor.FromArgb(255, 79, 141, 228),
            OxyColor.FromArgb(255, 255, 184, 55),
            OxyColor.FromArgb(255, 151, 137, 242)
        };

        public Chart(string title, string xAxisTitle, string yAxisTitle)
        {
            _plot = new PlotModel
            {
                Title = title,
                LegendPosition = LegendPosition.BottomLeft,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal
            };
            _plot.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = xAxisTitle,
                Minimum = 0,
                MajorGridlineStyle = LineStyle.Solid
            });
            _plot.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Title = yAxisTitle,
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid
            });
        }

        public Chart AddSeries<T>(IEnumerable<T> series, Func<T, double> yFunc = null, string name = "")
        {
            var colour = GetColorForIndex(_plot.Series.Count);
            var points = series.ToArray().Select((data, i) => new DataPoint(i, yFunc(data)));
            _plot.Series.Add(new OxyPlot.Series.LineSeries
            {
                ItemsSource = points,
                Title = name,
                Color = colour
            });
            _plot.Axes[0].Minimum = points.Min(x => x.X);
            _plot.Axes[0].Maximum = points.Max(x => x.X);
            _plot.Axes[1].Minimum = points.Min(x => x.Y);
            _plot.Axes[1].Maximum = points.Max(x => x.Y);

            return this;
        }

        public void Save(string filepath)
        {
            var exporter = new PngExporter { Width = 1500, Height = 400 };
            using (var stream = new MemoryStream())
            {
                exporter.Export(_plot, stream);
                var bytes = stream.ToArray();
                using (var filestream = new FileStream(filepath, FileMode.Create))
                    filestream.Write(bytes, 0, bytes.Length);
            }
        }

        private OxyColor GetColorForIndex(int index)
        {
            var i = index % _colours.Length;

            return _colours[i];
        }
    }
}

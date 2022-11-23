using OxyPlot;
using OxyPlot.Core.Drawing;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalysis.Models
{
    public class Chart
    {
        private readonly PlotModel _plot;
        private const int YAxis = 0;
        private double _minY;
        private static readonly OxyColor[] Colours = 
        {
            OxyColor.FromArgb(255, 149, 196, 235),
            OxyColor.FromArgb(255, 204, 133, 212),
            OxyColor.FromArgb(255, 102, 195, 144),
            OxyColor.FromArgb(255, 243, 155, 155),
            OxyColor.FromArgb(255, 79, 141, 228),
            OxyColor.FromArgb(255, 255, 184, 55),
            OxyColor.FromArgb(255, 151, 137, 242),
            OxyColor.FromArgb(255, 168, 242, 168),
            OxyColor.FromArgb(255, 155, 155, 155),
            OxyColor.FromArgb(255, 195, 82, 82),
        };

        public enum Type
        {
            Line,
            Point,
        }

        public Chart(string title, string xAxisTitle, string yAxisTitle)
        {
            _plot = new PlotModel
            {
                Title = title
            };
            _plot.Legends.Add(new Legend
            {
                LegendPosition = LegendPosition.BottomLeft,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal
            });
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

        public Chart AddSeries(IEnumerable<double> series, string name = "", Type type = Type.Line, OxyColor? colour = null)
        {
            colour ??= GetColorForIndex(_plot.Series.Count);
            return type switch
            {
                Type.Point => AddPointSeries(series, colour.Value, name),
                _          => AddLineSeries(series, colour.Value, name),
            };
        }

        private Chart AddPointSeries(IEnumerable<double> series, OxyColor colour, string name = "")
        {
            var points = series.Select((data, i) => new ScatterPoint(i, data)).ToArray();
            _plot.Series.Add(new OxyPlot.Series.ScatterSeries
            {
                ItemsSource = points,
                Title = name,
                MarkerSize = 1,
                MarkerFill = OxyColor.FromArgb(125, colour.R, colour.G, colour.B),
                MarkerType = MarkerType.Circle,
                MarkerStrokeThickness = 1,
                MarkerStroke = colour,
            });

            var minY = points.Min(x => x.Y);
            if (minY < _minY)
                _minY = minY;

            return this;
        }

        private Chart AddLineSeries(IEnumerable<double> series, OxyColor colour, string name = "")
        {
            var points = series.ToArray().Select((data, i) => new DataPoint(i, data));
            _plot.Series.Add(new OxyPlot.Series.LineSeries
            {
                ItemsSource = points,
                Title = name,
                Color = colour,
                MarkerSize = 2
            });

            var minY = points.Min(x => x.Y);
            if (minY < _minY)
                _minY = minY;

            return this;
        }

        public async Task Save(string filepath)
        {
            var bytes = ToByteArray();

            File.Delete(filepath);
            await using var filestream = new FileStream(filepath, FileMode.Create);
            await filestream.WriteAsync(bytes, 0, bytes.Length);
        }

        public byte[] ToByteArray()
        {
            _plot.Axes[YAxis].Minimum = _minY;

            using var stream = new MemoryStream();
            var exporter = new PngExporter { Width = 1500, Height = 400 };
            exporter.Export(_plot, stream);
            
            return stream.ToArray();
        }

        private static OxyColor GetColorForIndex(int index)
        {
            var i = index % Colours.Length;

            return Colours[i];
        }
    }
}

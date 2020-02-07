﻿using OxyPlot;
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
        private const int YAxis = 0;
        private double _minY;
        private static readonly OxyColor[] _colours = new[]
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

            var minY = points.Min(x => x.Y);
            if (minY < _minY)
                _minY = minY;

            return this;
        }

        public void Save(string filepath)
        {
            _plot.Axes[YAxis].Minimum = _minY;

            var exporter = new PngExporter { Width = 1500, Height = 400 };

            using var stream = new MemoryStream();
            exporter.Export(_plot, stream);
            var bytes = stream.ToArray();

            File.Delete(filepath);
            using var filestream = new FileStream(filepath, FileMode.Create);
            filestream.Write(bytes, 0, bytes.Length);
        }

        private OxyColor GetColorForIndex(int index)
        {
            var i = index % _colours.Length;

            return _colours[i];
        }
    }
}
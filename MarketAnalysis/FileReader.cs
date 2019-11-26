using MarketAnalysis.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MarketAnalysis
{
    public class FileReader
    {
        private readonly string _path;

        public FileReader(string path)
        {
            _path = path;
        }

        public List<MarketData> ReadOpeningPrices()
        {
            var results = new List<MarketData>();
            using (var reader = new StreamReader(_path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var provider = System.Globalization.CultureInfo.InvariantCulture;
                    var dateStr = values[0].ToString().Replace("\"", "");
                    if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", provider, System.Globalization.DateTimeStyles.None, out DateTime date))
                        DateTime.TryParseExact(dateStr, "MMM-dd-yyyy", provider, System.Globalization.DateTimeStyles.None, out date);

                    var priceStr = values[1].ToString().Replace("\"", "");
                    if (!decimal.TryParse(priceStr, out decimal price))
                        continue;

                    var volumeStr = values[5].ToString().Replace("\"", "");
                    decimal.TryParse(volumeStr.Replace("M", "").Replace("B", ""), out decimal volume);
                    if (volumeStr.Last() == 'B')
                        volume = volume * 1000;

                    results.Add(new MarketData
                    {
                        Date = date,
                        Price = price,
                        Volume = volume
                    });
                }
            }
            results = results.OrderBy(x => x.Date).ToList();
            for (int i = 1; i < results.Count; i++)
            {
                var delta = results[i].Price - results[i - 1].Price;
                results[i].Delta = delta;
            }

            return results;
        }
    }
}

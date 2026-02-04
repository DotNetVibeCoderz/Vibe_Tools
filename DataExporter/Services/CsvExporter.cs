using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using DataExporter.Core;

namespace DataExporter.Services
{
    public class CsvExporter : IExporter
    {
        public string Format => "csv";

        public void Export(IEnumerable<dynamic> data, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }
    }
}

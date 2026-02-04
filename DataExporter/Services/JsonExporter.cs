using System.Collections.Generic;
using System.IO;
using DataExporter.Core;
using Newtonsoft.Json;

namespace DataExporter.Services
{
    public class JsonExporter : IExporter
    {
        public string Format => "json";

        public void Export(IEnumerable<dynamic> data, string filePath)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}

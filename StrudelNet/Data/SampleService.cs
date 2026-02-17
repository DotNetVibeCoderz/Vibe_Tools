using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StrudelNet.Data
{
    public class SampleService
    {
        public List<string> GetSamples()
        {
            string samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples");
            if (!Directory.Exists(samplePath))
            {
                Directory.CreateDirectory(samplePath);
            }
            
            return Directory.GetFiles(samplePath, "*.strudel")
                            .Select(Path.GetFileName)
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => x!) // Null-forgiving
                            .ToList();
        }

        public async Task<string> LoadSample(string fileName)
        {
            string samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", fileName);
            if (File.Exists(samplePath))
            {
                return await File.ReadAllTextAsync(samplePath);
            }
            return "";
        }

        public async Task SaveSample(string fileName, string content)
        {
             string samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", fileName);
             await File.WriteAllTextAsync(samplePath, content);
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace DataExporter.Services
{
    public static class MaskingService
    {
        // Menyembunyikan data sensitif
        public static IEnumerable<dynamic> ApplyMasking(IEnumerable<dynamic> data, List<string>? fieldsToMask)
        {
            if (fieldsToMask == null || fieldsToMask.Count == 0)
                return data;

            var maskedList = new List<dynamic>();

            foreach (var row in data)
            {
                // Konversi row dynamic (DapperRow) ke Dictionary agar bisa diedit
                if (row is IDictionary<string, object> dict)
                {
                    // Fix warning CS8620 by manually copying instead of constructor
                    var newDict = new Dictionary<string, object?>();
                    foreach (var kvp in dict)
                    {
                        newDict[kvp.Key] = kvp.Value;
                    }

                    foreach (var field in fieldsToMask)
                    {
                        if (newDict.ContainsKey(field) && newDict[field] != null)
                        {
                            string? original = newDict[field]?.ToString();
                            if (original != null)
                            {
                                if (original.Length > 2)
                                {
                                    // Masking sederhana: J****y
                                    newDict[field] = original.Substring(0, 1) + new string('*', original.Length - 2) + original.Substring(original.Length - 1);
                                }
                                else
                                {
                                    newDict[field] = "**";
                                }
                            }
                        }
                    }
                    maskedList.Add(newDict);
                }
                else
                {
                    maskedList.Add(row);
                }
            }

            return maskedList;
        }
    }
}

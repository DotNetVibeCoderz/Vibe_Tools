using System.Collections.Generic;
using System.Data;

namespace DataExporter.Core
{
    // Kontrak untuk mengambil data dari berbagai database (SQL Server, MySQL, dll)
    public interface IDataSource
    {
        IEnumerable<dynamic> GetData(string query);
        void Execute(string query, object? parameters = null);
        string ConnectionString { get; }
    }

    // Kontrak untuk melakukan export ke berbagai format
    public interface IExporter
    {
        string Format { get; }
        void Export(IEnumerable<dynamic> data, string filePath);
    }
}

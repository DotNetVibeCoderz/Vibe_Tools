using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using DataExporter.Core;

namespace DataExporter.Services
{
    // Implementasi untuk SQLite
    public class SqliteDataSource : IDataSource
    {
        public string ConnectionString { get; private set; }

        public SqliteDataSource(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IEnumerable<dynamic> GetData(string query)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                return conn.Query(query);
            }
        }

        public void Execute(string query, object? parameters = null)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                conn.Execute(query, parameters);
            }
        }
    }
}

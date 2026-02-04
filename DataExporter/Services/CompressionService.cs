using System.IO;
using System.IO.Compression;

namespace DataExporter.Services
{
    public static class CompressionService
    {
        public static void CompressFile(string sourceFilePath, string outputZipPath)
        {
            // Pastikan file lama dihapus jika ada
            if (File.Exists(outputZipPath))
                File.Delete(outputZipPath);

            using (var zip = ZipFile.Open(outputZipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(sourceFilePath, Path.GetFileName(sourceFilePath));
            }
        }
    }
}

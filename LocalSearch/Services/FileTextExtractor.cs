using UglyToad.PdfPig;
using System.IO;
using System.Text;
using System.Linq;

namespace LocalSearch.Services
{
    public static class FileTextExtractor
    {
        public static string ExtractText(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLower();

            try 
            {
                if (ext == ".pdf")
                {
                    return ExtractFromPdf(filePath);
                }
                else if (IsTextFile(ext))
                {
                    return File.ReadAllText(filePath);
                }
            }
            catch 
            {
                // Ignore errors for now (locked files, etc)
                return string.Empty;
            }

            return string.Empty;
        }

        private static bool IsTextFile(string ext)
        {
            string[] textExtensions = { 
                ".txt", ".md", ".xml", ".json", ".csv", ".log",
                ".cs", ".js", ".ts", ".py", ".java", ".c", ".cpp", ".h", ".go", ".rs", ".php", ".html", ".css", ".sql", ".vb", ".bat", ".ps1", ".sh" 
            };
            return textExtensions.Contains(ext);
        }

        private static string ExtractFromPdf(string filePath)
        {
            var sb = new StringBuilder();
            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    foreach (var page in document.GetPages())
                    {
                        sb.Append(page.Text);
                        sb.Append(" ");
                    }
                }
            }
            catch { }
            return sb.ToString();
        }
    }
}
namespace LocalSearch.Models
{
    public class SearchResultItem
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty; // Potongan teks yang cocok
        public float Score { get; set; } // Skor relevansi
        public string FileType { get; set; } = string.Empty;
    }
}
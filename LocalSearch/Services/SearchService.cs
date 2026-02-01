using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LocalSearch.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LocalSearch.Services
{
    public class SearchService
    {
        private const string IndexDirectory = "Local_Index_DB";
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private FSDirectory _directory;
        private StandardAnalyzer _analyzer;

        public SearchService()
        {
            var indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IndexDirectory);
            _directory = FSDirectory.Open(indexPath);
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
        }

        public async Task<int> IndexDirectoryAsync(string rootPath, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
                indexConfig.OpenMode = OpenMode.CREATE; // Overwrite index for simplicity in this demo

                int count = 0;

                using (var writer = new IndexWriter(_directory, indexConfig))
                {
                    if (System.IO.Directory.Exists(rootPath))
                    {
                        var files = System.IO.Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
                        
                        foreach (var file in files)
                        {
                            progress?.Report($"Indexing: {Path.GetFileName(file)}");
                            
                            var textContent = FileTextExtractor.ExtractText(file);
                            if (string.IsNullOrWhiteSpace(textContent)) continue;

                            var doc = new Document();
                            // Stored = kept in index for retrieval, Indexed = searchable
                            doc.Add(new StringField("path", file, Field.Store.YES)); 
                            doc.Add(new TextField("filename", Path.GetFileName(file), Field.Store.YES));
                            doc.Add(new TextField("content", textContent, Field.Store.YES)); // Content is indexed but also stored for snippets (heavy, but ok for local)
                            
                            writer.AddDocument(doc);
                            count++;
                        }
                    }
                    writer.Flush(triggerMerge: false, applyAllDeletes: false);
                    writer.Commit();
                }
                return count;
            });
        }

        public List<SearchResultItem> Search(string queryText, bool semanticSearch)
        {
            var results = new List<SearchResultItem>();
            if (string.IsNullOrWhiteSpace(queryText) || !DirectoryReader.IndexExists(_directory)) 
                return results;

            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);
                Query query;

                if (semanticSearch)
                {
                    // "Semantic" simulation using Fuzzy Search (Levenstein Distance)
                    // Allows typos and close matches.
                    var parser = new MultiFieldQueryParser(AppLuceneVersion, new[] { "content", "filename" }, _analyzer);
                    // Append tilde ~ to terms for fuzzy search
                    string fuzzyQuery = queryText.Trim();
                    if (!fuzzyQuery.Contains("~")) fuzzyQuery += "~";
                    
                    try {
                        query = parser.Parse(fuzzyQuery);
                    } catch {
                        // Fallback
                        query = parser.Parse(QueryParser.Escape(queryText));
                    }
                }
                else
                {
                    // Exact Keyword Search
                    var parser = new QueryParser(AppLuceneVersion, "content", _analyzer);
                    try {
                         query = parser.Parse(queryText);
                    } catch {
                        query = parser.Parse(QueryParser.Escape(queryText));
                    }
                }

                // Check if any docs match
                var hits = searcher.Search(query, 50).ScoreDocs;

                foreach (var hit in hits)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    var content = foundDoc.Get("content");
                    
                    // Simple snippet generation
                    string snippet = string.Empty;
                    if (!string.IsNullOrEmpty(content))
                    {
                         snippet = content.Length > 200 ? content.Substring(0, 200) + "..." : content;
                    }

                    results.Add(new SearchResultItem
                    {
                        FilePath = foundDoc.Get("path"),
                        FileName = foundDoc.Get("filename"),
                        Score = hit.Score,
                        Snippet = snippet,
                        FileType = Path.GetExtension(foundDoc.Get("filename"))
                    });
                }
            }

            return results;
        }
    }
}
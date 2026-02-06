using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HtmlToPdf
{
    public static class TemplateProcessor
    {
        // Regex to find tags like [$Name], [$Address]
        private static readonly Regex FieldRegex = new Regex(@"\[\$(?<name>.*?)\]", RegexOptions.Compiled);

        public static List<string> ParseFields(string html)
        {
            var matches = FieldRegex.Matches(html);
            var fields = new HashSet<string>();

            foreach (Match match in matches)
            {
                fields.Add(match.Groups["name"].Value);
            }

            return fields.ToList();
        }

        public static string Process(string html, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            string result = html;
            foreach (var kvp in values)
            {
                // Replace [$Key] with Value
                result = result.Replace($"[${kvp.Key}]", kvp.Value);
            }

            return result;
        }
        
        public static string RepeatTemplate(string html, int count)
        {
             if (count <= 1) return html;
             
             // Create a grid system layout
             System.Text.StringBuilder sb = new System.Text.StringBuilder();
             
             // Extract style/head
             string headContent = "";
             var headMatch = Regex.Match(html, @"<head[^>]*>(.*?)</head>", RegexOptions.Singleline);
             if (headMatch.Success)
             {
                 headContent = headMatch.Groups[1].Value;
             }
             
             // Extract body content approximation
             var bodyMatch = Regex.Match(html, @"<body[^>]*>(.*?)</body>", RegexOptions.Singleline);
             string bodyContent = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

             sb.Append("<!DOCTYPE html><html><head>");
             sb.Append(headContent);
             
             // Add Grid CSS
             sb.Append("<style>");
             sb.Append(@"
                html, body { 
                    margin: 0; 
                    padding: 0; 
                    width: 100%; 
                    height: 100%; 
                }
                .grid-container {
                    display: flex;
                    flex-wrap: wrap;
                    justify-content: flex-start;
                    align-content: flex-start;
                    gap: 10px;
                    padding: 10px;
                    width: 100%;
                    box-sizing: border-box;
                }
                .grid-item {
                    flex: 0 0 auto;
                    box-sizing: border-box;
                    break-inside: avoid;
                    page-break-inside: avoid;
                    margin-bottom: 10px;
                }
                /* Print adjustments */
                @media print {
                    .grid-container {
                        padding: 0;
                    }
                    .grid-item {
                        break-inside: avoid;
                        page-break-inside: avoid;
                    }
                }
             ");
             sb.Append("</style>");
             sb.Append("</head><body>");
             
             sb.Append("<div class='grid-container'>");

             for (int i = 0; i < count; i++)
             {
                 sb.Append("<div class='grid-item'>");
                 sb.Append(bodyContent);
                 sb.Append("</div>");
             }
             
             sb.Append("</div>"); // Close grid-container
             sb.Append("</body></html>");
             
             return sb.ToString();
        }
    }
}
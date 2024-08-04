using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WAIModelDownloader.Jobs;

namespace WAIModelDownloader
{
    public class JobDataExtractor
    {
        private static readonly string[] ModelTypes = { "lora", "controlnet", "checkpoints" };

        public void ExtractJobData(DownloadModelJob job, string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract type if not already set
            if (string.IsNullOrEmpty(job.ModelType.ToString()))
            {
                string modelType = ExtractType(doc) ?? "Lora";
                if (Enum.TryParse(modelType, out ModelType parsedModelType))
                {
                    job.ModelType = parsedModelType;
                }
                else
                {
                    job.ModelType = ModelType.Lora; // Default to Lora if parsing fails
                }
            }

            // Extract download links
            var linkNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, 'api/download/models')]");
            if (linkNodes != null && linkNodes.Count > 0)
            {
                foreach (var linkNode in linkNodes)
                {
                    string href = linkNode.GetAttributeValue("href", "");
                    if (href.Contains("api/download/models"))
                    {
                        string downloadUrl = "https://civitai.com" + href;
                        job.ModelDownloadLink = downloadUrl;
                        break; // Assume the first matching link is the correct one
                    }
                }
            }

            // If no download link is found, use the backup builder
            if (string.IsNullOrEmpty(job.ModelDownloadLink))
            {
                job.ModelDownloadLink = BackupJobDownloadLinkBuilder(job);
            }
        }

        public string BackupJobDownloadLinkBuilder(DownloadModelJob job)
        {
            string template = "/api/download/models/{MODELV}?type=Model&format=SafeTensor";
            string modelVersion = ExtractModelVersionId(job.ModelUrl);
            return "https://civitai.com" + template.Replace("{MODELV}", modelVersion);
        }

        public string ExtractModelVersionId(string url)
        {
            var match = Regex.Match(url, @"[?&]modelVersionId=(\d+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private string ExtractType(HtmlDocument doc)
        {
            var typeLabelNode = doc.DocumentNode.SelectSingleNode("//div[text()='Type']");
            if (typeLabelNode != null)
            {
                var typeContainer = typeLabelNode.ParentNode.ParentNode.SelectSingleNode("following-sibling::td/div");
                if (typeContainer != null)
                {
                    var typeNode = FindMatchingTextNode(typeContainer);
                    if (typeNode != null)
                    {
                        return typeNode.InnerText.Trim();
                    }
                }
            }
            return null;
        }

        private HtmlNode FindMatchingTextNode(HtmlNode node)
        {
            foreach (var descendant in node.Descendants())
            {
                if (!descendant.HasChildNodes && !string.IsNullOrWhiteSpace(descendant.InnerText))
                {
                    string innerText = descendant.InnerText.Trim().ToLower();
                    foreach (var modelType in ModelTypes)
                    {
                        if (innerText.Contains(modelType))
                        {
                            return descendant;
                        }
                    }
                }
            }

            return null;
        }

        public string ExtractNameFromUrl(string url)
        {
            var uri = new Uri(url);
            string name = uri.Segments.Last().Replace("-", "_"); // Replace dashes with underscores

            // Remove special characters
            StringBuilder sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}

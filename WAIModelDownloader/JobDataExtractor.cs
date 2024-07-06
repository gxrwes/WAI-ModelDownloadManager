using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text;
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
            if (string.IsNullOrEmpty(job.ModelType))
            {
                job.ModelType = ExtractType(doc) ?? "lora";
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

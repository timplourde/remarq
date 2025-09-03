using Markdig;
using System;
using System.IO;

namespace Remarq
{
    public class DocConverter
    {
        private readonly string _template;

        public DocConverter(string template)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw new ArgumentException("Template cannot be null or empty", nameof(template));
            }
            _template = template;
        }

        public string Convert(string markdown, string title)
        {
            var (frontMatter, bodyContent) = FrontMatter.Extract(markdown);
            
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdown.ToHtml(bodyContent, pipeline)
                .Replace(".md\"", ".html\"") // Convert markdown links within quotes
                .Replace(".md)", ".html)"); // Convert markdown links in parentheses
            
            return _template
                .Replace("{{BODY}}", html)
                .Replace("{{TITLE}}", frontMatter?.Title ?? title);
        }
    }
}
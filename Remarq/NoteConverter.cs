using MarkdownSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Remarq
{
    internal class NoteConverter
    {
        private string _htmlTemplate;

        public NoteConverter(string htmlTemplatePath)
        {
            if (!File.Exists(htmlTemplatePath))
            {
                throw new ArgumentException($"Template file {htmlTemplatePath} does not exist");
            }

            var template = File.ReadAllText(htmlTemplatePath);
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("HTML template is empty");
            }
            _htmlTemplate = template;
            
        }

        public string Convert(string markdown, string fileName)
        {
            var mdConverter = new Markdown();
            var htmlBody = mdConverter.Transform(markdown);
            htmlBody = htmlBody.Replace(".md", ".html");
            return _htmlTemplate.Replace("{{BODY}}", htmlBody).Replace("{{TITLE}}", fileName);
        }
    }
}

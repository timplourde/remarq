using System;
using System.IO;
using Xunit;

namespace Remarq.Tests
{
    public class NoteConverterTests : IDisposable
    {
        private readonly string _testTemplateDir;
        private readonly string _validTemplatePath;
        private readonly string _validTemplateContent = @"<!DOCTYPE html>
<html>
<head><title>{{TITLE}}</title></head>
<body>{{BODY}}</body>
</html>";

        public NoteConverterTests()
        {
            _testTemplateDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testTemplateDir);
            _validTemplatePath = Path.Combine(_testTemplateDir, "template.html");
            File.WriteAllText(_validTemplatePath, _validTemplateContent);
        }

        [Fact]
        public void Constructor_WithValidTemplate_ShouldSucceed()
        {
            // Act & Assert
            var converter = new NoteConverter(_validTemplatePath);
            Assert.NotNull(converter);
        }

        [Fact]
        public void Constructor_WithNonExistentTemplate_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testTemplateDir, "nonexistent.html");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new NoteConverter(nonExistentPath));
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyTemplate_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyTemplatePath = Path.Combine(_testTemplateDir, "empty.html");
            File.WriteAllText(emptyTemplatePath, "");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new NoteConverter(emptyTemplatePath));
            Assert.Contains("HTML template is empty", exception.Message);
        }

        [Fact]
        public void Convert_WithSimpleMarkdown_ShouldReplaceTemplateVariables()
        {
            // Arrange
            var converter = new NoteConverter(_validTemplatePath);
            var markdown = "# Hello World\nThis is a test.";
            var fileName = "test.md";

            // Act
            var result = converter.Convert(markdown, fileName);

            // Assert
            Assert.Contains("<h1>Hello World</h1>", result);
            Assert.Contains("<p>This is a test.</p>", result);
            Assert.Contains("<title>test.md</title>", result);
            Assert.Contains("<!DOCTYPE html>", result);
        }

        [Fact]
        public void Convert_WithMarkdownLinks_ShouldConvertMdToHtml()
        {
            // Arrange
            var converter = new NoteConverter(_validTemplatePath);
            var markdown = "[Link to other note](other-note.md)";
            var fileName = "test.md";

            // Act
            var result = converter.Convert(markdown, fileName);

            // Assert
            Assert.Contains("other-note.html", result);
            Assert.DoesNotContain("other-note.md", result);
        }

        [Fact]
        public void Convert_WithEmptyMarkdown_ShouldHandleGracefully()
        {
            // Arrange
            var converter = new NoteConverter(_validTemplatePath);
            var markdown = "";
            var fileName = "empty.md";

            // Act
            var result = converter.Convert(markdown, fileName);

            // Assert
            Assert.Contains("<title>empty.md</title>", result);
            Assert.Contains("<!DOCTYPE html>", result);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testTemplateDir))
            {
                Directory.Delete(_testTemplateDir, true);
            }
        }
    }
}
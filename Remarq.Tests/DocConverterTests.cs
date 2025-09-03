using System;
using System.IO;
using Xunit;

namespace Remarq.Tests
{
    public class DocConverterTests : IDisposable
    {
        private readonly string _validTemplateContent = @"<!DOCTYPE html>
<html>
<head><title>{{TITLE}}</title></head>
<body>{{BODY}}</body>
</html>";

        public DocConverterTests()
        {
            // No setup needed since we pass template content directly
        }

        [Fact]
        public void Constructor_WithValidTemplate_ShouldSucceed()
        {
            // Act & Assert
            var converter = new DocConverter(_validTemplateContent);
            Assert.NotNull(converter);
        }

        [Fact]
        public void Constructor_WithNullTemplate_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new DocConverter(null));
        }

        [Fact]
        public void Constructor_WithEmptyTemplate_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new DocConverter(string.Empty));
        }

        [Fact]
        public void Convert_WithSimpleMarkdown_ShouldReplaceTemplateVariables()
        {
            // Arrange
            var converter = new DocConverter(_validTemplateContent);
            var markdown = "# Hello World\nThis is a test.";
            var fileName = "test.md";

            // Act
            var result = converter.Convert(markdown, fileName);

            // Assert
            Assert.Contains("<h1 id=\"hello-world\">Hello World</h1>", result);
            Assert.Contains("<p>This is a test.</p>", result);
            Assert.Contains("<title>test.md</title>", result);
            Assert.Contains("<!DOCTYPE html>", result);
        }

        [Fact]
        public void Convert_WithMarkdownLinks_ShouldConvertMdToHtml()
        {
            // Arrange
            var converter = new DocConverter(_validTemplateContent);
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
            var converter = new DocConverter(_validTemplateContent);
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
            // Nothing to dispose now that we don't create temporary files
        }
    }
}
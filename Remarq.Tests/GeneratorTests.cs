using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Remarq.Tests
{
    public class GeneratorTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _sourceDir;
        private readonly string _targetDir;
        private readonly string _templatePath;

        public GeneratorTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _sourceDir = Path.Combine(_testDir, "source");
            _targetDir = Path.Combine(_testDir, "target");
            _templatePath = Path.Combine(_testDir, "template.html");

            Directory.CreateDirectory(_testDir);
            Directory.CreateDirectory(_sourceDir);

            // Create a basic template
            var templateContent = @"<!DOCTYPE html>
<html>
<head><title>{{TITLE}}</title></head>
<body>{{BODY}}</body>
</html>";
            File.WriteAllText(_templatePath, templateContent);
        }

        [Fact]
        public void Constructor_WithValidPaths_ShouldSucceed()
        {
            // Act & Assert
            var generator = new Generator(_sourceDir, _targetDir, _templatePath);
            Assert.NotNull(generator);
        }

        [Fact]
        public void Constructor_WithNonExistentSourceDirectory_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentSource = Path.Combine(_testDir, "nonexistent");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Generator(nonExistentSource, _targetDir, _templatePath));
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public void Constructor_WithNonExistentTemplateFile_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentTemplate = Path.Combine(_testDir, "nonexistent-template.html");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new Generator(_sourceDir, _targetDir, nonExistentTemplate));
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task Generate_WithEmptySourceDirectory_ShouldReturn0()
        {
            // Arrange
            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(0, result);
            // The target directory is not created if there are no files to process
        }

        [Fact]
        public async Task Generate_WithMarkdownFiles_ShouldConvertToHtml()
        {
            // Arrange
            var markdownContent = "# Test Note\nThis is a test note.";
            var markdownPath = Path.Combine(_sourceDir, "test.md");
            File.WriteAllText(markdownPath, markdownContent);

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(1, result);
            
            var expectedHtmlPath = Path.Combine(_targetDir, "test.html");
            Assert.True(File.Exists(expectedHtmlPath));
            
            var htmlContent = File.ReadAllText(expectedHtmlPath);
            Assert.Contains("<h1 id=\"test-note\">Test Note</h1>", htmlContent);
            Assert.Contains("<p>This is a test note.</p>", htmlContent);
            Assert.Contains("<title>test.md</title>", htmlContent);
        }

        [Fact]
        public async Task Generate_WithNonMarkdownFiles_ShouldCopyAsIs()
        {
            // Arrange
            var txtContent = "This is a text file.";
            var txtPath = Path.Combine(_sourceDir, "notes.txt");
            File.WriteAllText(txtPath, txtContent);

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(1, result);
            
            var expectedTxtPath = Path.Combine(_targetDir, "notes.txt");
            Assert.True(File.Exists(expectedTxtPath));
            
            var copiedContent = File.ReadAllText(expectedTxtPath);
            Assert.Equal(txtContent, copiedContent);
        }

        [Fact]
        public async Task Generate_WithNestedDirectories_ShouldPreserveStructure()
        {
            // Arrange
            var nestedDir = Path.Combine(_sourceDir, "notes", "projects");
            Directory.CreateDirectory(nestedDir);
            
            var markdownContent = "# Project Note";
            var markdownPath = Path.Combine(nestedDir, "project1.md");
            File.WriteAllText(markdownPath, markdownContent);

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(1, result);
            
            var expectedHtmlPath = Path.Combine(_targetDir, "notes", "projects", "project1.html");
            Assert.True(File.Exists(expectedHtmlPath));
            
            var htmlContent = File.ReadAllText(expectedHtmlPath);
            Assert.Contains("<h1 id=\"project-note\">Project Note</h1>", htmlContent);
        }

        [Fact]
        public async Task Generate_WithMixedFiles_ShouldProcessCorrectly()
        {
            // Arrange
            // Create a markdown file
            var markdownContent = "# Markdown File";
            var markdownPath = Path.Combine(_sourceDir, "note.md");
            File.WriteAllText(markdownPath, markdownContent);

            // Create a text file
            var txtContent = "Plain text";
            var txtPath = Path.Combine(_sourceDir, "readme.txt");
            File.WriteAllText(txtPath, txtContent);

            // Create an image file (simulate)
            var imagePath = Path.Combine(_sourceDir, "image.png");
            File.WriteAllBytes(imagePath, new byte[] { 1, 2, 3, 4 });

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(3, result);
            
            // Check markdown was converted
            var htmlPath = Path.Combine(_targetDir, "note.html");
            Assert.True(File.Exists(htmlPath));
            var htmlContent = File.ReadAllText(htmlPath);
            Assert.Contains("<h1 id=\"markdown-file\">Markdown File</h1>", htmlContent);

            // Check text file was copied
            var copiedTxtPath = Path.Combine(_targetDir, "readme.txt");
            Assert.True(File.Exists(copiedTxtPath));
            Assert.Equal(txtContent, File.ReadAllText(copiedTxtPath));

            // Check image was copied
            var copiedImagePath = Path.Combine(_targetDir, "image.png");
            Assert.True(File.Exists(copiedImagePath));
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, File.ReadAllBytes(copiedImagePath));
        }

        [Fact]
        public async Task Generate_RecreatesTargetDirectory()
        {
            // Arrange
            // Create target directory with some existing files
            Directory.CreateDirectory(_targetDir);
            var existingFile = Path.Combine(_targetDir, "existing.txt");
            File.WriteAllText(existingFile, "should be deleted");

            // Create source file
            var markdownPath = Path.Combine(_sourceDir, "new.md");
            File.WriteAllText(markdownPath, "# New File");

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(1, result);
            Assert.False(File.Exists(existingFile)); // Old file should be gone
            Assert.True(File.Exists(Path.Combine(_targetDir, "new.html"))); // New file should exist
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
    }
}
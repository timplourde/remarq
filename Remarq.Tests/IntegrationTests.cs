using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Remarq.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _sourceDir;
        private readonly string _targetDir;
        private readonly string _templatePath;

        public IntegrationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _sourceDir = Path.Combine(_testDir, "source");
            _targetDir = Path.Combine(_testDir, "target");
            _templatePath = Path.Combine(_testDir, "template.html");

            Directory.CreateDirectory(_testDir);
            Directory.CreateDirectory(_sourceDir);

            // Use the same template structure as the sample
            var templateContent = @"<!doctype html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>{{TITLE}}</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 2em; }
        h1, h2, h3 { color: #333; }
    </style>
</head>
<body>
    {{BODY}}
</body>
</html>";
            File.WriteAllText(_templatePath, templateContent);
        }

        [Fact]
        public async Task EndToEnd_CompleteWorkflow_ShouldWorkCorrectly()
        {
            // Arrange - Create a structure similar to the sample
            var notesDir = Path.Combine(_sourceDir, "notes");
            var recipesDir = Path.Combine(_sourceDir, "recipes");
            Directory.CreateDirectory(notesDir);
            Directory.CreateDirectory(recipesDir);

            // Create markdown files
            var journalContent = @"# My Journal

This is my daily journal.

## Today's Activities
- Wrote some code
- [Read recipes](recipes/brownies.md)
- Planned next project";

            var browniesContent = @"# Brownies Recipe

## Ingredients
- 2 cups flour
- 1 cup sugar
- 1/2 cup cocoa powder

## Instructions
1. Mix ingredients
2. Bake at 350°F for 30 minutes

[Back to journal](../journal.md)";

            var projectContent = @"# Project Alpha

This is a top secret project.

## Status
- Planning phase
- Team: 3 developers";

            File.WriteAllText(Path.Combine(_sourceDir, "journal.md"), journalContent);
            File.WriteAllText(Path.Combine(recipesDir, "brownies.md"), browniesContent);
            File.WriteAllText(Path.Combine(notesDir, "project-alpha.md"), projectContent);

            // Create non-markdown files
            var readmeContent = "This is a readme file.";
            File.WriteAllText(Path.Combine(_sourceDir, "README.txt"), readmeContent);

            // Create binary file (image simulation)
            var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header bytes
            File.WriteAllBytes(Path.Combine(notesDir, "diagram.png"), imageData);

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act
            var result = await generator.Generate();

            // Assert
            Assert.Equal(5, result); // 3 markdown + 1 text + 1 image

            // Verify directory structure is preserved
            Assert.True(Directory.Exists(Path.Combine(_targetDir, "notes")));
            Assert.True(Directory.Exists(Path.Combine(_targetDir, "recipes")));

            // Verify markdown files were converted to HTML
            var journalHtmlPath = Path.Combine(_targetDir, "journal.html");
            Assert.True(File.Exists(journalHtmlPath));
            var journalHtml = File.ReadAllText(journalHtmlPath);
            Assert.Contains("<h1>My Journal</h1>", journalHtml);
            Assert.Contains("<h2>Today's Activities</h2>", journalHtml);
            Assert.Contains("recipes/brownies.html", journalHtml); // Links converted
            Assert.DoesNotContain("recipes/brownies.md", journalHtml);
            Assert.Contains("<title>journal.md</title>", journalHtml);

            var browniesHtmlPath = Path.Combine(_targetDir, "recipes", "brownies.html");
            Assert.True(File.Exists(browniesHtmlPath));
            var browniesHtml = File.ReadAllText(browniesHtmlPath);
            Assert.Contains("<h1>Brownies Recipe</h1>", browniesHtml);
            Assert.Contains("<h2>Ingredients</h2>", browniesHtml);
            Assert.Contains("<li>2 cups flour</li>", browniesHtml);
            Assert.Contains("../journal.html", browniesHtml); // Relative links converted

            var projectHtmlPath = Path.Combine(_targetDir, "notes", "project-alpha.html");
            Assert.True(File.Exists(projectHtmlPath));
            var projectHtml = File.ReadAllText(projectHtmlPath);
            Assert.Contains("<h1>Project Alpha</h1>", projectHtml);
            Assert.Contains("3 developers", projectHtml);

            // Verify non-markdown files were copied
            var readmePath = Path.Combine(_targetDir, "README.txt");
            Assert.True(File.Exists(readmePath));
            Assert.Equal(readmeContent, File.ReadAllText(readmePath));

            var imagePath = Path.Combine(_targetDir, "notes", "diagram.png");
            Assert.True(File.Exists(imagePath));
            Assert.Equal(imageData, File.ReadAllBytes(imagePath));

            // Verify template was applied correctly
            Assert.Contains("<!doctype html>", journalHtml);
            Assert.Contains("font-family: Arial", journalHtml);
            Assert.Contains("<meta charset=\"utf-8\">", journalHtml);
        }

        [Fact]
        public void EndToEnd_WithInvalidTemplate_ShouldThrowException()
        {
            // Arrange
            var invalidTemplatePath = Path.Combine(_testDir, "invalid.html");
            // Don't create the file

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new Generator(_sourceDir, _targetDir, invalidTemplatePath));
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public void EndToEnd_WithInvalidSourceDirectory_ShouldThrowException()
        {
            // Arrange
            var invalidSourcePath = Path.Combine(_testDir, "invalid");

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                new Generator(invalidSourcePath, _targetDir, _templatePath));
            Assert.Contains("does not exist", exception.Message);
        }

        [Fact]
        public async Task EndToEnd_RepeatedGeneration_ShouldOverwriteExistingFiles()
        {
            // Arrange
            var markdownContent = "# Original Content";
            var markdownPath = Path.Combine(_sourceDir, "test.md");
            File.WriteAllText(markdownPath, markdownContent);

            var generator = new Generator(_sourceDir, _targetDir, _templatePath);

            // Act - First generation
            await generator.Generate();
            var firstHtml = File.ReadAllText(Path.Combine(_targetDir, "test.html"));

            // Modify source and generate again
            var updatedContent = "# Updated Content";
            File.WriteAllText(markdownPath, updatedContent);
            await generator.Generate();
            var secondHtml = File.ReadAllText(Path.Combine(_targetDir, "test.html"));

            // Assert
            Assert.Contains("Original Content", firstHtml);
            Assert.Contains("Updated Content", secondHtml);
            Assert.DoesNotContain("Original Content", secondHtml);
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
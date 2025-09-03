using Xunit;

namespace Remarq.Tests;

public class FrontMatterTests
{
    [Fact]
    public void Extract_WithNoFrontMatter_ShouldReturnNullAndOriginalContent()
    {
        // Arrange
        var content = "# Hello World\nThis is a test.";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.Null(frontMatter);
        Assert.Equal(content, remainingContent);
    }

    [Fact]
    public void Extract_WithInvalidFrontMatterStart_ShouldReturnNullAndOriginalContent()
    {
        // Arrange
        var content = "Something\n---\ntitle: Test\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.Null(frontMatter);
        Assert.Equal(content, remainingContent);
    }

    [Fact]
    public void Extract_WithMissingFrontMatterEnd_ShouldReturnNullAndOriginalContent()
    {
        // Arrange
        var content = "---\ntitle: Test\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.Null(frontMatter);
        Assert.Equal(content, remainingContent);
    }

    [Fact]
    public void Extract_WithValidFrontMatter_ShouldReturnFrontMatterAndContent()
    {
        // Arrange
        var content = "---\ntitle: Test Title\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Equal("Test Title", frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }

    [Fact]
    public void Extract_WithEmptyFrontMatter_ShouldReturnEmptyFrontMatterAndContent()
    {
        // Arrange
        var content = "---\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Null(frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }

    [Fact]
    public void Extract_WithInvalidFrontMatterLine_ShouldSkipInvalidLine()
    {
        // Arrange
        var content = "---\ntitle: Test Title\ninvalid line\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Equal("Test Title", frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }

    [Fact]
    public void Extract_WithMultipleFrontMatterFields_ShouldOnlyExtractTitle()
    {
        // Arrange
        var content = "---\ntitle: Test Title\ndate: 2025-09-03\nauthor: John Doe\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Equal("Test Title", frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }

    [Fact]
    public void Extract_WithCaseInsensitiveTitle_ShouldExtractTitle()
    {
        // Arrange
        var content = "---\nTiTLe: Test Title\n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Equal("Test Title", frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }

    [Fact]
    public void Extract_WithExtraWhitespace_ShouldTrimValues()
    {
        // Arrange
        var content = "---\ntitle:    Test Title    \n---\n# Content";

        // Act
        var (frontMatter, remainingContent) = FrontMatter.Extract(content);

        // Assert
        Assert.NotNull(frontMatter);
        Assert.Equal("Test Title", frontMatter.Title);
        Assert.Equal("# Content", remainingContent);
    }
}

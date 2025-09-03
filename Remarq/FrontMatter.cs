using System;
using System.Text.RegularExpressions;
#nullable enable

namespace Remarq;

public class FrontMatter
{
    public string? Title { get; set; }

    public static (FrontMatter?, string) Extract(string content)
    {
        if (!content.StartsWith("---\n"))
            return (null, content);

        var endIndex = content.IndexOf("\n---\n", 4);
        if (endIndex == -1)
        {
            // Check for empty front matter
            if (content.StartsWith("---\n---\n"))
            {
                return (new FrontMatter(), content.Substring(8)); // Skip both markers and both newlines
            }
            return (null, content);
        }

        var frontMatter = new FrontMatter();
        var frontMatterContent = content.Substring(4, endIndex - 4);
        
        foreach (var line in frontMatterContent.Split('\n'))
        {
            var parts = line.Split(':', 2);
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (key.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                frontMatter.Title = value;
            }
        }

        var remainingContent = content.Substring(endIndex + 5);
        return (frontMatter, remainingContent);
    }
}

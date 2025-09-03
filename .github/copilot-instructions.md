# Remarq - Markdown to HTML Converter

Remarq is a .NET 8.0 console application that converts markdown notes to HTML using the Markdig library. It processes entire directory trees of markdown files and generates corresponding HTML files using a customizable template.

**ALWAYS follow these instructions first and only fallback to additional search or bash commands when the information here is incomplete or incorrect.**

## Working Effectively

### Prerequisites and Environment Setup
- Ensure .NET 8.0 SDK is installed: `dotnet --version` should return 8.0.x
- The project targets .NET 8.0 and uses the Markdig NuGet package for markdown processing
- No additional dependencies or complex setup required

### Core Build and Run Commands
**NEVER CANCEL any build or test commands - they complete quickly (under 30 seconds)**

1. **Restore dependencies** (6-10 seconds):
   ```bash
   dotnet restore
   ```

2. **Build the project** (8-12 seconds):
   ```bash
   dotnet build
   ```

3. **Run the application with sample data** (1-2 seconds):
   ```bash
   dotnet run --project Remarq sample/source test_output sample/template.html
   ```
   This processes 7 sample markdown files and generates HTML output.

4. **Build Release version** (1-3 seconds):
   ```bash
   dotnet publish -c Release
   ```

5. **Create single-file executable** (2-8 seconds):
   ```bash
   # For Linux/macOS
   dotnet publish Remarq/Remarq.csproj -p:PublishSingleFile=true -r linux-x64 -c Release --self-contained true -o publish_linux -f net8.0

   # For Windows (using PowerShell)
   pwsh -File publish.ps1
   # OR manually:
   dotnet publish Remarq/Remarq.csproj -p:PublishSingleFile=true -r win-x64 -c Release --self-contained true -o publish -f net8.0
   ```

### Application Usage
The application requires exactly 3 arguments:
```bash
remarq [source-directory] [target-directory] [html-template-path]
```

Example: `./Remarq sample/source test_output sample/template.html`

The app will:
- Convert all `.md` files to `.html` using the template
- Copy all other files as-is
- Maintain directory structure in the output
- Replace `{{BODY}}` in template with generated HTML
- Replace `{{TITLE}}` in template with filename

## Code Quality and Formatting

### Formatting Issues (CRITICAL)
**The codebase currently has formatting issues that MUST be addressed before committing changes:**

```bash
# Check formatting (will show errors if formatting is incorrect)
dotnet format --verify-no-changes

# Fix formatting automatically
dotnet format
```

**Always run `dotnet format` before committing any changes** to ensure code style compliance.

## Manual Validation and Testing

### Required Validation Steps
**After making any changes, ALWAYS perform these validation steps:**

1. **Build and run with sample data:**
   ```bash
   dotnet build
   dotnet run --project Remarq sample/source test_output sample/template.html
   ```

2. **Verify HTML generation:**
   ```bash
   # Check that files were created
   ls -la test_output/

   # Verify HTML structure (should contain CSS and proper HTML)
   head -30 test_output/journal.html
   ```

3. **Test published executable:**
   ```bash
   dotnet publish -c Release -o publish_test
   cd publish_test
   ./Remarq ../sample/source ../test_output2 ../sample/template.html
   cd ..
   ```

4. **Verify markdown link conversion:**
   The application automatically converts `.md` links to `.html` links in the generated HTML.

### No Unit Tests Available
- The project currently has unit and integration tests in the Remarq.Tests folder
- The tests cover various aspects of the application, including file processing and markdown conversion
- The tests use the xUnit library

## Project Structure and Navigation

### Key Directories and Files
```
/
├── Remarq/                    # Main project directory
├── Remarq.Tests/             # Test Suite
├── sample/                   # Test data and template
│   ├── source/              # Sample markdown files (journal, recipes, notes)
│   └── template.html        # HTML template with {{BODY}} and {{TITLE}} placeholders
├── Remarq.sln              # Visual Studio solution file
├── publish.ps1             # PowerShell script for Windows single-file publishing
├── README.md               # Basic usage and build instructions
└── .gitignore              # Excludes bin/, obj/, publish/, test outputs
```

## Common Development Tasks

### Making Code Changes
1. Always start by ensuring the code builds and runs successfully
2. Make minimal, focused changes
3. Run `dotnet format` to fix any formatting issues
4. Test with sample data to ensure functionality works
5. Build release version to ensure no compilation issues

### Debugging and Troubleshooting
- Use Visual Studio Code or Visual Studio with the solution file
- Launch configuration is in `Remarq/Properties/launchSettings.json`
- Debug arguments point to sample data: `sample/source test_output sample/template.html`
- Check generated HTML files to verify markdown processing
- Common issues: missing template file, invalid source directory, formatting problems

### Cross-Platform Considerations
- The application works on Windows, Linux, and macOS
- Use `dotnet publish` with appropriate `-r` flags for platform-specific builds
- PowerShell script (`publish.ps1`) works on any platform with PowerShell Core
- Single-file executables are ~65MB due to self-contained deployment

### Performance Characteristics
- Very fast processing: 7 sample files process in ~1.5 seconds
- Build times are consistently under 10 seconds
- Publish operations complete in 2-8 seconds
- Memory usage is minimal for typical document sets

## Frequently Referenced Commands

```bash
# Quick validation sequence (run this after any changes)
dotnet format && dotnet build && dotnet run --project Remarq sample/source test_output sample/template.html

# Full build and test sequence
dotnet restore && dotnet build && dotnet run --project Remarq sample/source test_output sample/template.html && ls -la test_output/

# Check file differences in output
diff -r sample/source test_output --exclude="*.md" --include="*.html"
```

### Output Examples
When processing the sample data, expect:
- `test_output/journal.html` - Main journal file
- `test_output/recipes/` - Recipe directory with brownies.html and wedding cake.html  
- `test_output/notes/` - Notes directory with project files
- All HTML files should have proper CSS styling and dark/light mode support
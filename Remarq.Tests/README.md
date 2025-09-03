# Remarq Tests

This directory contains automated tests for the Remarq markdown-to-HTML converter.

## Test Structure

### Unit Tests
- **NoteConverterTests.cs** - Tests for the `NoteConverter` class
  - Template validation (file existence, non-empty content)
  - Markdown to HTML conversion functionality
  - Template variable replacement ({{TITLE}} and {{BODY}})
  - Link conversion (.md to .html)

- **GeneratorTests.cs** - Tests for the `Generator` class
  - Constructor validation (source directory existence)
  - File processing workflows
  - Directory structure preservation
  - Mixed file type handling (markdown + other files)
  - Target directory recreation

### Integration Tests
- **IntegrationTests.cs** - End-to-end workflow tests
  - Complete conversion scenarios with nested directories
  - Error handling for invalid inputs
  - Repeated generation and file overwriting
  - Real-world use case simulation

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=NoteConverterTests"
```

## Test Coverage

The tests cover:
- ✅ Core markdown conversion functionality
- ✅ Template processing and variable replacement
- ✅ File system operations (read/write/copy)
- ✅ Directory structure preservation
- ✅ Error handling for invalid inputs
- ✅ Edge cases (empty files, missing files, etc.)
- ✅ End-to-end workflow scenarios

## Test Dependencies

- **xUnit** - Test framework
- **Temporary directories** - Tests use isolated temp directories to avoid conflicts
- **Sample data** - Tests create their own test data to ensure reliability
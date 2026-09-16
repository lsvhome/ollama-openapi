using System.IO;
using System.Text;
using OllamaOpenApi.Service.Tools;
using Xunit;

namespace OllamaOpenApi.Service.Tests;

public class FileEditToolsTests : IDisposable
{
    private readonly string _testFile = "test_edit_file.txt";

    public FileEditToolsTests()
    {
        File.WriteAllText(_testFile, "Hello World! This is a test file.");
    }

    public void Dispose()
    {
        if (File.Exists(_testFile))
        {
            File.Delete(_testFile);
        }
    }

    [Fact]
    public void FileReplaceText_ShouldReplaceTextSuccessfully()
    {
        // Arrange
        string oldText = "Hello World!";
        string newText = "Greetings Universe!";

        // Act
        string result = ToolConsole.FileReplaceText(_testFile, oldText, newText);

        // Assert
        Assert.Contains("Successfully replaced", result);
        string contents = File.ReadAllText(_testFile);
        Assert.Contains(newText, contents);
        Assert.DoesNotContain(oldText, contents);
    }

    [Fact]
    public void FileReplaceText_ShouldReturnWarningIfTextNotFound()
    {
        // Arrange
        string oldText = "Non-existent text";
        string newText = "Replacement";

        // Act
        string result = ToolConsole.FileReplaceText(_testFile, oldText, newText);

        // Assert
        Assert.Contains("Warning: Text 'Non-existent text' not found", result);
        string contents = File.ReadAllText(_testFile);
        Assert.Equal("Hello World! This is a test file.", contents);
    }

    [Fact]
    public void FileReplaceRegex_ShouldReplaceWithRegexSuccessfully()
    {
        // Arrange
        // Match "World" followed by "!"
        string regexPattern = "World!";
        string replacement = "Galaxy!";

        // Act
        string result = ToolConsole.FileReplaceRegex(_testFile, regexPattern, replacement);

        // Assert
        Assert.Contains("Successfully replaced matches of pattern 'World!'", result);
        string contents = File.ReadAllText(_testFile);
        Assert.Contains("Hello Galaxy! This is a test file.", contents);
    }

    [Fact]
    public void FileReplaceRegex_ShouldReturnWarningIfNoMatchFound()
    {
        // Arrange
        string regexPattern = "Missing";
        string replacement = "Found";

        // Act
        string result = ToolConsole.FileReplaceRegex(_testFile, regexPattern, replacement);

        // Assert
        Assert.Contains("Warning: No matches found for pattern 'Missing'", result);
        string contents = File.ReadAllText(_testFile);
        Assert.Equal("Hello World! This is a test file.", contents);
    }

    [Fact]
    public void FileReplaceRegex_ShouldHandleCaptureGroups()
    {
        // Arrange
        // Replace "Hello World" with "Greetings World" using capture group
        // We need to modify the file first to have something that matches better
        File.WriteAllText(_testFile, "Hello World");
        string regexPattern = "(Hello) (World)";
        string replacement = "Greetings $2";

        // Act
        string result = ToolConsole.FileReplaceRegex(_testFile, regexPattern, replacement);

        // Assert
        Assert.Contains("Successfully replaced", result);
        string contents = File.ReadAllText(_testFile);
        Assert.Equal("Greetings World", contents);
    }

    [Fact]
    public void FileReplaceText_ShouldReturnErrorIfFileNotFound()
    {
        // Arrange
        string nonExistentFile = "non_existent_file_12345.txt";
        string oldText = "test";
        string newText = "new";

        // Act
        string result = ToolConsole.FileReplaceText(nonExistentFile, oldText, newText);

        // Assert
        Assert.Contains($"Error: File not found at {nonExistentFile}", result);
    }

    [Fact]
    public void FileReplaceRegex_ShouldReturnErrorIfFileNotFound()
    {
        // Arrange
        string nonExistentFile = "non_existent_file_12345.txt";
        string regexPattern = "test";
        string replacement = "new";

        // Act
        string result = ToolConsole.FileReplaceRegex(nonExistentFile, regexPattern, replacement);

        // Assert
        Assert.Contains($"Error: File not found at {nonExistentFile}", result);
    }
}

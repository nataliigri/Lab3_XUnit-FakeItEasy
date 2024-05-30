using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using Xunit;

public class ProgramTests
{
    [Fact]
    public void GetUniqueVowelWords_ReturnsUniqueVowelWords()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        string[] result = program.GetUniqueVowelWords(testContent);

        // Assert
        Assert.Equal(new[] { "aei", "o", "u", "aeiou", "uoiea" }, result);
    }

    [Fact]
    public void GetUniqueVowelWords_ReturnsEmptyArrayForNoVowelWords()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "bcd fgh";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        string[] result = program.GetUniqueVowelWords(testContent);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Run_HandlesIOException()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Throws<IOException>();

        var program = new Program(fakeFileReader);

        using (var sw = new StringWriter())
        {
            Console.SetOut(sw);

            // Act
            program.Run("fakePath");

            // Assert
            var result = sw.ToString().Trim();
            Assert.Equal("Unable to read the file: Exception of type 'System.IO.IOException' was thrown.", result);
        }
    }

    [Fact]
    public void Run_PrintsFilteredWords()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea bcd fgh";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        using (var sw = new StringWriter())
        {
            Console.SetOut(sw);

            // Act
            program.Run("fakePath");

            // Assert
            var result = sw.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Contains("Filtered Words:", result);
            Assert.Contains("aei", result);
            Assert.Contains("o", result);
            Assert.Contains("u", result);
            Assert.Contains("aeiou", result);
            Assert.Contains("uoiea", result);
            Assert.DoesNotContain("bcd", result);
            Assert.DoesNotContain("fgh", result);
        }
    }

    [Fact]
    public void Run_CallsReadFileOnce()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        program.Run("existingFile.txt");

        // Assert
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Run_CallsReadFileThreeTimes()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        program.Run("existingFile.txt");
        program.Run("existingFile.txt");
        program.Run("existingFile.txt");

        // Assert
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public void Run_CallsReadFileAtLeastOnce()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        program.Run("existingFile.txt");

        // Assert
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).MustHaveHappened(Repeated.AtLeast.Once);
    }

    [Fact]
    public void Run_CallsReadFileWithSpecificPath()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.That.Matches(path => path == "existingFile.txt"))).Returns(testContent);

        var program = new Program(fakeFileReader);

        // Act
        program.Run("existingFile.txt");

        // Assert
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.That.Matches(path => path == "existingFile.txt"))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void Run_GeneratesExceptionOnInvalidPath()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Throws<IOException>();

        var program = new Program(fakeFileReader);

        using (var sw = new StringWriter())
        {
            Console.SetOut(sw);

            // Act
            program.Run("invalidPath");

            // Assert
            var result = sw.ToString().Trim();
            Assert.StartsWith("Unable to read the file:", result);
        }
    }

    [Fact]
    public void Run_InjectsFileReaderUsingAnnotations()
    {
        // Arrange & Act
        var program = new Program();

        // Assert
        Assert.NotNull(program._fileReader);
    }

    [Fact]
    public void Run_SpyChecksMethodCall()
    {
        // Arrange
        var fakeFileReader = A.Fake<IFileReader>();
        string testContent = "aei o u aeiou uoiea";
        A.CallTo(() => fakeFileReader.ReadFile(A<string>.Ignored)).Returns(testContent);

        var programSpy = A.Fake<Program>(builder => builder.WithArgumentsForConstructor(() => new Program(fakeFileReader)).Wrapping(new Program(fakeFileReader)));

        // Act
        programSpy.Run("existingFile.txt");

        // Assert
        A.CallTo(() => programSpy.GetUniqueVowelWords(testContent)).MustHaveHappenedOnceExactly();
    }
}
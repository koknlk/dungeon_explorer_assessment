using DungeonExplorerBackend.Services;
using Xunit;

namespace Tests.UnitTests.Services
    {
    public class InputSanitizerTests
        {
        private readonly InputSanitizer _sanitizer;

        public InputSanitizerTests()
            {
            _sanitizer = new InputSanitizer();
            }

        [Theory]
        [InlineData("Normal Name", "Normal Name")]
        [InlineData("Name with spaces", "Name with spaces")]
        [InlineData("Name-with-hyphens", "Name-with-hyphens")]
        [InlineData("Name_with_underscores", "Name_with_underscores")]
        [InlineData("Name123", "Name123")]
        public void Sanitize_ReturnsCleanInput_ForSafeStrings(string input, string expected)
            {
            // Act
            var result = _sanitizer.Sanitize(input);

            // Assert
            Assert.Equal(expected, result);
            }

        [Theory]
        [InlineData("Name<script>alert('xss')</script>", "Namescriptalertxssscript")]
        [InlineData("Name\"with quotes", "Namewith quotes")]
        [InlineData("Name'with single quotes", "Namewith single quotes")]
        [InlineData("Name<with brackets>", "Namewith brackets")]
        [InlineData("Name&with&ampersand", "Namewithampersand")]
        public void Sanitize_RemovesDangerousCharacters_ForXssPrevention(string input, string expected)
            {
            // Act
            var result = _sanitizer.Sanitize(input);

            // Assert
            Assert.Equal(expected, result);
            }

        [Theory]
        [InlineData("   Leading spaces", "Leading spaces")]
        [InlineData("Trailing spaces   ", "Trailing spaces")]
        [InlineData("   Both spaces   ", "Both spaces")]
        public void Sanitize_TrimsWhitespace(string input, string expected)
            {
            // Act
            var result = _sanitizer.Sanitize(input);

            // Assert
            Assert.Equal(expected, result);
            }

        [Fact]
        public void Sanitize_ReturnsEmptyString_ForNullInput()
            {
            // Act
            var result = _sanitizer.Sanitize(null!);

            // Assert
            Assert.Equal(string.Empty, result);
            }

        [Fact]
        public void Sanitize_ReturnsEmptyString_ForWhitespaceInput()
            {
            // Act
            var result = _sanitizer.Sanitize("   ");

            // Assert
            Assert.Equal(string.Empty, result);
            }
        }
    }
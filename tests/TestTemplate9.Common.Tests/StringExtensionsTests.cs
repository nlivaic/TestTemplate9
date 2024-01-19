using TestTemplate9.Common.Extensions;
using Xunit;

namespace TestTemplate9.Common.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void StringExtensionsTests_CanCapitalizeNonNullStrings_Successfully()
        {
            // Arrange
            var target = "target string";

            // Act
            var result = target.CapitalizeFirstLetter();

            // Assert
            Assert.Equal("Target string", result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void StringExtensionsTests_SkipsNullOrWhiteSpaceStrings_Successfully(string target)
        {
            // Act
            var result = target.CapitalizeFirstLetter();

            // Assert
            Assert.Equal(target, result);
        }
    }
}

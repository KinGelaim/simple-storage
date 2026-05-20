using SimpleStorage.Tests.Utils;

namespace SimpleStorage.Tests;

public sealed class CommandParserTests
{
    [Fact]
    public void Success_WhenCommandWithThreeArguments()
    {
        // Arrange
        var command = "SET user:1 data";

        // Act
        var result = CommandParser.Parse(command.ToBytes());

        // Assert
        Assert.Equal("SET", BytesConverter.ToString(result.Command));
        Assert.Equal("user:1", BytesConverter.ToString(result.Key));
        Assert.Equal("data", BytesConverter.ToString(result.Value));
    }

    [Fact]
    public void Success_WhenCommandWithTwoArguments()
    {
        // Arrange
        var command = "GET user:1";

        // Act
        var result = CommandParser.Parse(command.ToBytes());

        // Assert
        Assert.Equal("GET", BytesConverter.ToString(result.Command));
        Assert.Equal("user:1", BytesConverter.ToString(result.Key));
        Assert.True(result.Value.IsEmpty);
    }

    [Fact]
    public void ReturnsDefault_WhenCommandWithOneArgument()
    {
        // Arrange
        var command = "DELETE";

        // Act
        var result = CommandParser.Parse(command.ToBytes());

        // Assert
        Assert.True(result.Command.IsEmpty);
        Assert.True(result.Key.IsEmpty);
        Assert.True(result.Value.IsEmpty);
    }
}
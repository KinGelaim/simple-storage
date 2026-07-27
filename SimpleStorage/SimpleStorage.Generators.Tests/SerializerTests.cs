namespace SimpleStorage.Generators.Tests;

public sealed class SerializerTests
{
    [Fact]
    public void TestSerialization()
    {
        // Arrange
        var person = new Person { Id = 42, Name = "Misha", BirthDate = new DateTime(1995, 02, 02) };

        // Act
        using var stream = new MemoryStream();
        person.SerializeToBinary(stream);

        var data = stream.ToArray();

        // Assert
        Assert.NotEmpty(data);

        using var mem = new MemoryStream(data);
        using var reader = new BinaryReader(mem);

        var idRead = reader.ReadInt32();
        var nameRead = reader.ReadString();
        var dateTicks = reader.ReadInt64();
        var dateRead = new DateTime(dateTicks);

        Assert.Equal(person.Id, idRead);
        Assert.Equal(person.Name, nameRead);
        Assert.Equal(person.BirthDate, dateRead);
    }
}
using System.Text;

namespace SimpleStorage.Generators.Tests;

public sealed class SerializerTests
{
    [Fact]
    public void TestSerialization()
    {
        // Arrange
        var person = new Person { Id = 42, Name = "Misha", BirthDate = new DateTime(1995, 02, 02), Data = [1, 2, 3] };

        // Act
        using var stream = new MemoryStream();
        person.SerializeToBinary(stream);

        var data = stream.ToArray();

        // Assert
        Assert.NotEmpty(data);

        using var mem = new MemoryStream(data);
        using var reader = new BinaryReader(mem);

        var idRead = reader.ReadInt32();
        var nameReadLength = reader.ReadInt32();
        var nameReadBytes = reader.ReadBytes(nameReadLength);
        var nameRead = Encoding.UTF8.GetString(nameReadBytes);
        var dateTicks = reader.ReadInt64();
        var dateRead = new DateTime(dateTicks);
        var dataReadLength = reader.ReadInt32();
        var dataRead = reader.ReadBytes(dataReadLength);

        Assert.Equal(person.Id, idRead);
        Assert.Equal(person.Name, nameRead);
        Assert.Equal(person.BirthDate, dateRead);
        Assert.Equal(person.Data, dataRead);
    }

    [Fact]
    public void TestDeserialization()
    {
        // Arrange
        var person = new Person { Id = 42, Name = "Misha", BirthDate = new DateTime(1995, 02, 02) };
        using var mem = new MemoryStream();
        using var writer = new BinaryWriter(mem);
        writer.Write(person.Id);
        var nameBytes = Encoding.UTF8.GetBytes(person.Name);
        writer.Write(nameBytes.Length);
        writer.Write(nameBytes);
        writer.Write(person.BirthDate.Ticks);
        writer.Write(0);

        var data = mem.ToArray();

        // Act
        using var stream = new MemoryStream(data);
        var personResult = Person.DeserializeFromBinary(stream);

        // Assert
        Assert.NotNull(personResult);
        Assert.Equal(person.Id, personResult.Id);
        Assert.Equal(person.Name, personResult.Name);
        Assert.Equal(person.BirthDate, personResult.BirthDate);
        Assert.Equal(person.Data, personResult.Data);
    }
}
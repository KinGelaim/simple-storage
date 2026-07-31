namespace SimpleStorage.Generators.Tests;

[GenerateBinarySerializer]
public partial class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public byte[] Data { get; set; } = [];
}
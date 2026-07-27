namespace SimpleStorage.Generators.Tests;

[GenerateBinarySerializer]
public partial class Person
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime BirthDate { get; set; }
}
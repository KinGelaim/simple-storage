using BenchmarkDotNet.Attributes;
using System.Text.Json;

namespace SimpleStorage.Generators.Benchmark;

[MemoryDiagnoser]
public class SerializationBenchmarks
{
    private readonly UserProfile _userProfile;

    public SerializationBenchmarks() =>
        _userProfile = new UserProfile
        {
            Id = 123,
            UserName = "Misha",
            CreatedAt = new DateTime(2026, 07, 27)
        };

    [Benchmark]
    public byte[] BinarySerialization()
    {
        using var ms = new MemoryStream();
        _userProfile.SerializeToBinary(ms);
        return ms.ToArray();
    }

    [Benchmark]
    public string JsonSerialization() => JsonSerializer.Serialize(_userProfile);
}
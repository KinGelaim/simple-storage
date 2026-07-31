using BenchmarkDotNet.Attributes;

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

    [Benchmark(Baseline = true)]
    public string NewtonsoftJsonSerialization() =>
        Newtonsoft.Json.JsonConvert.SerializeObject(_userProfile);

    [Benchmark]
    public string SystemTextJsonSerialization() =>
        System.Text.Json.JsonSerializer.Serialize(_userProfile);

    [Benchmark]
    public byte[] SourceGeneratorSerialization()
    {
        using var ms = new MemoryStream();
        _userProfile.SerializeToBinary(ms);
        return ms.ToArray();
    }

    private readonly byte[] _sharedBuffer = new byte[1024];

    [Benchmark]
    public int SourceGeneratorSerializationWithZeroAlloc()
    {
        using var ms = new MemoryStream(_sharedBuffer);
        _userProfile.SerializeToBinary(ms);
        return (int)ms.Position;
    }
}
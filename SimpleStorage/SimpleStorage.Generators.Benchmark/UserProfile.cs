namespace SimpleStorage.Generators.Benchmark;

[GenerateBinarySerializer]
public sealed partial class UserProfile
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public required DateTime CreatedAt { get; set; }
}
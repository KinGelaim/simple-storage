using SimpleStorage.Generators;

namespace SimpleStorage.DTO;

/// <summary>
/// Информация о профиле пользователя
/// </summary>
[GenerateBinarySerializer]
public sealed partial class UserProfile
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
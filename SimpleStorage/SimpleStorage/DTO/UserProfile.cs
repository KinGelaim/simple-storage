namespace SimpleStorage.DTO;

/// <summary>
/// Информация о профиле пользователя
/// </summary>
public sealed class UserProfile
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
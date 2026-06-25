namespace SimpleStorage.Models;

/// <summary>
/// Тип команды
/// </summary>
internal enum CommandType
{
    /// <summary>
    /// Получение значения
    /// </summary>
    Get,

    /// <summary>
    /// Сохранение/обновление значения
    /// </summary>
    Set,

    /// <summary>
    /// Удаление значения
    /// </summary>
    Delete
}
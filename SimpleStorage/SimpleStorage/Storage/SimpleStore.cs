namespace SimpleStorage.Storage;

/// <summary>
/// Базовое хранилище
/// </summary>
internal sealed class SimpleStore
{
    private readonly Dictionary<string, byte[]> _data = [];

    /// <summary>
    /// Добавление или обновление значения по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Set(string key, byte[] value) => _data[key] = value;

    /// <summary>
    /// Возвращает значение по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение по ключу или null, если ключ не найден</returns>
    public byte[]? Get(string key)
    {
        _data.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Удаляет ключ и значение
    /// </summary>
    /// <param name="key">Ключ</param>
    public void Delete(string key) => _data.Remove(key);
}
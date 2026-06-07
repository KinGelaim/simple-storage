namespace SimpleStorage.Storage;

/// <summary>
/// Базовое хранилище
/// </summary>
internal sealed class SimpleStore : IDisposable
{
    private readonly Dictionary<string, byte[]> _data = [];
    private readonly ReaderWriterLockSlim _lock = new();

    private long _setCount = 0;
    private long _getCount = 0;
    private long _deleteCount = 0;

    /// <summary>
    /// Добавление или обновление значения по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Set(string key, byte[] value)
    {
        _lock.EnterWriteLock();
        try
        {
            _data[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Возвращает значение по ключу
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение по ключу или null, если ключ не найден</returns>
    public byte[]? Get(string key)
    {
        _lock.EnterReadLock();
        try
        {
            _data.TryGetValue(key, out var value);
            return value;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Удаляет ключ и значение
    /// </summary>
    /// <param name="key">Ключ</param>
    public void Delete(string key)
    {
        _lock.EnterWriteLock();
        try
        {
            _data.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long SetCount, long GetCount, long DeleteCount) GetStatistics()
        => (_setCount, _getCount, _deleteCount);

    public void Dispose() => _lock.Dispose();
}
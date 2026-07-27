using SimpleStorage.DTO;
using System.Text.Json;

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
    /// <param name="userProfile">Значение</param>
    public void Set(string key, UserProfile userProfile)
    {
        Interlocked.Increment(ref _setCount);

        _lock.EnterWriteLock();
        try
        {
            using var stream = new MemoryStream();
            userProfile.SerializeToBinary(stream);
            _data[key] = stream.ToArray();
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
    public UserProfile? Get(string key)
    {
        Interlocked.Increment(ref _getCount);

        _lock.EnterReadLock();
        try
        {
            _data.TryGetValue(key, out var bytes);
            if (bytes is null)
            {
                return null;
            }

            var value = JsonSerializer.Deserialize<UserProfile>(bytes);
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
        Interlocked.Increment(ref _deleteCount);

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

    /// <summary>
    /// Получает статистику обращений к хранилищу
    /// </summary>
    public (long SetCount, long GetCount, long DeleteCount) GetStatistics()
        => (Interlocked.Read(ref _setCount),
            Interlocked.Read(ref _getCount),
            Interlocked.Read(ref _deleteCount));

    public void Dispose() => _lock.Dispose();
}
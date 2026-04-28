using System.Text;
using System.Text.Json;

namespace ParsingData
{
    public class SimpleStore: IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<string, byte[]> _storage = new Dictionary<string, byte[]>();

        private long _setCount;
        private long _getCount;
        private long _deleteCount;

        public (long SetCount, long GetCount, long DeleteCount) GetStatistics() =>
            new(_setCount, _getCount, _deleteCount);

        /// <summary> Добавляет или обновляет значения по ключу </summary>
        public void Set(string key, UserProfile profile)
        {
            _lock.EnterWriteLock();
            try
            {
                using (var stream = new MemoryStream()) {
                    //var value = profile.SerializeToBinary(stream);
                    var value = JsonSerializer.SerializeToUtf8Bytes(profile);
                    _storage[key] = value;
                }
                Interlocked.Increment(ref _setCount);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary> Возвращает значение по ключу или null, если ключ не найден </summary>
        public UserProfile Get(string key)
        {
            _lock.EnterReadLock();
            try
            {
                var value = _storage.TryGetValue(key, out var val) ? val : null;
                Interlocked.Increment(ref _getCount);
                var result = value != null ? JsonSerializer.Deserialize<UserProfile>(value) : null;
                return result;
            }
            finally 
            { 
                _lock.ExitReadLock(); 
            }
        }

        /// <summary> Удаляет ключ и значение </summary>
        public void Delete(string key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_storage.ContainsKey(key))
                    _storage.Remove(key);
                Interlocked.Increment(ref _deleteCount);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}

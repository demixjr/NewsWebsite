using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace UI.Tests.Helpers
{
    /// <summary>
    /// Тестова реалізація ISession для юніт-тестів
    /// </summary>
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();
        private readonly string _id = Guid.NewGuid().ToString();

        public string Id => _id;

        public bool IsAvailable => true;

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[]? value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }

    }

    /// <summary>
    /// Extension методи для роботи з об'єктами в Session (як у вашому проекті)
    /// </summary>
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T? value)
        {
            if (value == null)
            {
                session.Remove(key);
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(value);
                var bytes = Encoding.UTF8.GetBytes(json);
                session.Set(key, bytes);
            }
            catch
            {
                // In tests, silently skip serialization errors
            }
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            if (session.TryGetValue(key, out var bytes) && bytes != null && bytes.Length > 0)
            {
                try
                {
                    var json = Encoding.UTF8.GetString(bytes);
                    if (string.IsNullOrWhiteSpace(json)) return default;
                    return JsonSerializer.Deserialize<T>(json);
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }

    }
}

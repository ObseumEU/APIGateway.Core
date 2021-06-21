using System;
using System.Runtime.Caching;

namespace APIGateway.Core.Cache
{
    public interface ICacheService
    {
        T Get<T>(string cacheKey);
        void Set(string cacheKey, object item, double minutes);
    }

    public class InMemoryCache : ICacheService
    {
        public T Get<T>(string cacheKey)
        {
            return MemoryCache.Default.Get(cacheKey) is T ? (T) MemoryCache.Default.Get(cacheKey) : default;
        }

        public void Set(string cacheKey, object item, double minutes)
        {
            if (item != null) MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddMinutes(minutes));
        }
    }
}
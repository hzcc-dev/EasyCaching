﻿namespace EasyCaching.Memcached
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using Enyim.Caching;
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Linq;

    /// <summary>
    /// Default memcached caching provider.
    /// </summary>
    public class DefaultMemcachedCachingProvider : IEasyCachingProvider
    {
        /// <summary>
        /// The memcached client.
        /// </summary>
        private readonly IMemcachedClient _memcachedClient;

        /// <summary>
        /// <see cref="T:EasyCaching.Memcached.DefaultMemcachedCachingProvider"/>
        /// is distributed cache.
        /// </summary>
        public bool IsDistributedCache => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EasyCaching.Memcached.DefaultMemcachedCachingProvider"/> class.
        /// </summary>
        /// <param name="memcachedClient">Memcached client.</param>
        public DefaultMemcachedCachingProvider(IMemcachedClient memcachedClient)
        {
            this._memcachedClient = memcachedClient;
        }

        /// <summary>
        /// Get the specified cacheKey, dataRetriever and expiration.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = _memcachedClient.Get(this.HandleCacheKey(cacheKey)) as T;
            if (result != null)
            {
                return new CacheValue<T>(result, true);
            }

            var item = dataRetriever?.Invoke();
            if (item != null)
            {
                this.Set(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the specified cacheKey, dataRetriever and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="dataRetriever">Data retriever.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            var result = await _memcachedClient.GetValueAsync<T>(this.HandleCacheKey(cacheKey));
            if (result != null)
            {
                return new CacheValue<T>(result, true);
            }

            var item = await dataRetriever?.Invoke();
            if (item != null)
            {
                await this.SetAsync(cacheKey, item, expiration);
                return new CacheValue<T>(item, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public CacheValue<T> Get<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = _memcachedClient.Get(this.HandleCacheKey(cacheKey)) as T;
            if (result != null)
            {
                return new CacheValue<T>(result, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Gets the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<CacheValue<T>> GetAsync<T>(string cacheKey) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            var result = await _memcachedClient.GetValueAsync<T>(this.HandleCacheKey(cacheKey));
            if (result != null)
            {
                return new CacheValue<T>(result, true);
            }
            else
            {
                return CacheValue<T>.NoValue;
            }
        }

        /// <summary>
        /// Remove the specified cacheKey.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public void Remove(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            _memcachedClient.Remove(this.HandleCacheKey(cacheKey));
        }

        /// <summary>
        /// Removes the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task RemoveAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            await _memcachedClient.RemoveAsync(this.HandleCacheKey(cacheKey));
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(cacheKey), cacheValue, expiration);
        }

        /// <summary>
        /// Sets the specified cacheKey, cacheValue and expiration async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(cacheKey), cacheValue, expiration);
        }

        /// <summary>
        /// Exists the specified cacheKey.
        /// </summary>
        /// <returns>The exists.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public bool Exists(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return _memcachedClient.TryGet(this.HandleCacheKey(cacheKey), out object obj);
        }

        /// <summary>
        /// Existses the specified cacheKey async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public async Task<bool> ExistsAsync(string cacheKey)
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));

            return await Task.FromResult(_memcachedClient.TryGet(this.HandleCacheKey(cacheKey), out object obj));
        }

        /// <summary>
        /// Refresh the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void Refresh<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            this.Remove(cacheKey);
            this.Set(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Refreshs the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task RefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNullOrWhiteSpace(cacheKey, nameof(cacheKey));
            ArgumentCheck.NotNull(cacheValue, nameof(cacheValue));
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));

            await this.RemoveAsync(cacheKey);
            await this.SetAsync(cacheKey, cacheValue, expiration);
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix.
        /// </summary>
        /// <remarks>
        /// Before using the method , you should follow this link 
        /// https://github.com/memcached/memcached/wiki/ProgrammingTricks#namespacing
        /// and confirm that you use the namespacing when you set and get the cache.
        /// </remarks>
        /// <param name="prefix">Prefix of CacheKey.</param>
        public void RemoveByPrefix(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var oldPrefixKey = _memcachedClient.Get(prefix)?.ToString();

            var newValue = DateTime.UtcNow.Ticks.ToString();

            if (oldPrefixKey.Equals(newValue))
            {
                newValue = string.Concat(newValue, new Random().Next(9).ToString());
            }
            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(prefix), newValue, new TimeSpan(0, 0, 0));
        }

        /// <summary>
        /// Removes cached item by cachekey's prefix async.
        /// </summary>
        /// <remarks>
        /// Before using the method , you should follow this link 
        /// https://github.com/memcached/memcached/wiki/ProgrammingTricks#namespacing
        /// and confirm that you use the namespacing when you set and get the cache.
        /// </remarks>
        /// <param name="prefix">Prefix of CacheKey.</param>
        /// <returns></returns>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            ArgumentCheck.NotNullOrWhiteSpace(prefix, nameof(prefix));

            var oldPrefixKey = _memcachedClient.Get(prefix)?.ToString();

            var newValue = DateTime.UtcNow.Ticks.ToString();

            if (oldPrefixKey.Equals(newValue))
            {
                newValue = string.Concat(newValue, new Random().Next(9).ToString());
            }
            await _memcachedClient.StoreAsync(Enyim.Caching.Memcached.StoreMode.Set, this.HandleCacheKey(prefix), newValue, new TimeSpan(0, 0, 0));
        }

        /// <summary>
        /// Handle the cache key of memcached limititaion
        /// </summary>
        /// <param name="cacheKey">Cache Key</param>
        /// <returns></returns>
        private string HandleCacheKey(string cacheKey)
        {
            // Memcached has a 250 character limit
            // Following memcached.h in https://github.com/memcached/memcached/
            if (cacheKey.Length >= 250)
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
                    return Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
            }

            return cacheKey;
        }

        /// <summary>
        /// Sets all.
        /// </summary>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public void SetAll<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            foreach (var item in values)
            {
                Set(item.Key, item.Value, expiration);
            }
        }

        /// <summary>
        /// Sets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="values">Values.</param>
        /// <param name="expiration">Expiration.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task SetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration) where T : class
        {
            ArgumentCheck.NotNegativeOrZero(expiration, nameof(expiration));
            ArgumentCheck.NotNullAndCountGTZero(values, nameof(values));

            var tasks = new List<Task>();
            foreach (var item in values)
            {
                tasks.Add(SetAsync(item.Key, item.Value, expiration));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The all.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetAll<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var values = _memcachedClient.Get<T>(cacheKeys);
            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in values)
            {
                if (item.Value != null)
                    result.Add(item.Key, new CacheValue<T>(item.Value, true));
                else
                    result.Add(item.Key, CacheValue<T>.NoValue);
            }

            return result;
        }

        /// <summary>
        /// Gets all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<IDictionary<string, CacheValue<T>>> GetAllAsync<T>(IEnumerable<string> cacheKeys) where T : class
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var values = await _memcachedClient.GetAsync<T>(cacheKeys);
            var result = new Dictionary<string, CacheValue<T>>();

            foreach (var item in values)
            {
                if (item.Value != null)
                    result.Add(item.Key, new CacheValue<T>(item.Value, true));
                else
                    result.Add(item.Key, CacheValue<T>.NoValue);
            }

            return result;
        }

        /// <summary>
        /// Gets the by prefix.
        /// </summary>
        /// <returns>The by prefix.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IDictionary<string, CacheValue<T>> GetByPrefix<T>(string prefix) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the by prefix async.
        /// </summary>
        /// <returns>The by prefix async.</returns>
        /// <param name="prefix">Prefix.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public Task<IDictionary<string, CacheValue<T>>> GetByPrefixAsync<T>(string prefix) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="cacheKeys">Cache keys.</param>
        public void RemoveAll(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            foreach (var item in cacheKeys.Distinct())
                Remove(item);
        }

        /// <summary>
        /// Removes all async.
        /// </summary>
        /// <returns>The all async.</returns>
        /// <param name="cacheKeys">Cache keys.</param>
        public async Task RemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            ArgumentCheck.NotNullAndCountGTZero(cacheKeys, nameof(cacheKeys));

            var tasks = new List<Task>();
            foreach (var item in cacheKeys.Distinct())
                tasks.Add(RemoveAsync(item));

            await Task.WhenAll(tasks);
        }
    }
}

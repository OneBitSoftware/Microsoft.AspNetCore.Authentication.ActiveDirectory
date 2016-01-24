namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using System;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// An in-memory cache for the login handshakes
    /// </summary>
    internal class AuthenticationStateCache
    {
        #region fields
        private IMemoryCache Cache;

        /// <summary>
        /// Expiration time of a login attempt state in minutes,
        /// defaults to 2
        /// </summary>
        public int ExpirationTime { get; set; }
        #endregion

        /// <summary>
        /// Create an authentication state cache
        /// </summary>
        /// <param name="name"></param>
        public AuthenticationStateCache()
        {
            this.Cache = new MemoryCache(new MemoryCacheOptions());
            this.ExpirationTime = 2;
        }

        /// <summary>
        /// Try to get a state by its key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool TryGet(string key, out HandshakeState state)
        {
            object tmp;
            if (Cache.TryGetValue(key, out tmp))
            {
                if (tmp != null)
                {
                    state = (HandshakeState)tmp;
                    return true;
                }
            }

            state = default(HandshakeState);
            return false;
        }

        /// <summary>
        /// Add a new state to the cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        public void Add(string key, HandshakeState state)
        {
            this.Cache.Set(key, state, GetCacheEntryOptions(this.ExpirationTime));
        }

        /// <summary>
        /// Add a new state to the cache and set a custom cache item policy
        /// </summary>
        /// <param name="key"></param>
        /// <param name="state"></param>
        /// <param name="policy"></param>
        public void Add(string key, HandshakeState state, MemoryCacheEntryOptions options)
        {
            this.Cache.Set(key, state, options);
        }

        /// <summary>
        /// Remove a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void TryRemove(string key)
        {
            this.Cache.Remove(key);
        }

        #region Helpers
        /// <summary>
        /// Gets a cache item policy.
        /// </summary>
        /// <param name="minutes">Absolute expiration time in x minutes</param>
        /// <returns></returns>
        private static MemoryCacheEntryOptions GetCacheEntryOptions(int minutes)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.Normal,
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minutes)
                
                //RemovedCallback = (item) =>
                //{
                //    // dispose cached item at removal
                //    var asDisposable = item.CacheItem as IDisposable;
                //    if (asDisposable != null)
                //        asDisposable.Dispose();
                //}
            };
            return cacheEntryOptions;
        }
        #endregion

    }

}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Infinispan.Hotrod.Core;

namespace Infinispan.Hotrod.Caching
{
    public class InfinispanCache : IDistributedCache, IDisposable
    {
        private InfinispanDG cluster;
        private Cache<string, byte[]> cache;
        public InfinispanCache(IOptions<InfinispanCacheOptions> optionsAccessor)
        {
            cluster = optionsAccessor.Value.Cluster;
            cache = cluster.NewCache<string, byte[]>(new StringMarshaller(), new IdentityMarshaller(), optionsAccessor.Value.CacheName);
        }
        public byte[] Get(string key)
        {
            return cache.Get(key).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return await cache.Get(key);
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).Wait();
        }
        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            ValueWithMetadata<byte[]> vwm = await cache.GetWithMetadata(key);
            if (vwm == null || vwm.Lifespan == -1)
                return;
            ExpirationTime lifeS = new ExpirationTime();
            lifeS.Unit = TimeUnit.SECONDS;
            lifeS.Value = vwm.Lifespan;
            ExpirationTime maxIdle = null;
            if (vwm.MaxIdle != -1)
            {
                maxIdle = new ExpirationTime();
                maxIdle.Unit = TimeUnit.SECONDS;
                maxIdle.Value = vwm.MaxIdle;
            }
            await cache.Put(key, vwm.Value, lifeS, maxIdle);
        }

        public void Remove(string key)
        {
            _ = cache.Remove(key).Result;
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await cache.Remove(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _ = cache.Put(key, value).Result;
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            ExpirationTime lifeSpan = getLifeSpan(options);
            ExpirationTime maxIdle = getMaxIdle(options);

            await cache.Put(key, value, lifeSpan, maxIdle);
        }

        private ExpirationTime getMaxIdle(DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                return new ExpirationTime { Unit = TimeUnit.SECONDS, Value = (long)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds };
            }
            if (options.AbsoluteExpiration.HasValue)
            {
                var lifeSpanTime = options.AbsoluteExpiration - DateTimeOffset.UtcNow;
                return new ExpirationTime { Unit = TimeUnit.SECONDS, Value = (long)(lifeSpanTime.Value.TotalSeconds) };
            }
            if (options.SlidingExpiration.HasValue)
            {
                var lifeSpanTime = options.SlidingExpiration;
                return new ExpirationTime { Unit = TimeUnit.SECONDS, Value = (long)(lifeSpanTime.Value.TotalSeconds) };
            }
            return null;
        }

        private ExpirationTime getLifeSpan(DistributedCacheEntryOptions options)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }

    internal class IdentityMarshaller : Marshaller<byte[]>
    {
        public override byte[] marshall(byte[] t)
        {
            if (t == null)
            {
                return null;
            }
            byte[] r = new byte[t.Length];
            Array.ConstrainedCopy(t, 0, r, 0, t.Length);
            return r;
        }

        public override byte[] unmarshall(byte[] buff)
        {
            if (buff == null)
            {
                return null;
            }
            byte[] r = new byte[buff.Length];
            Array.ConstrainedCopy(buff, 0, r, 0, buff.Length);
            return r;
        }
    }
}

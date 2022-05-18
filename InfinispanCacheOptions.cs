using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infinispan.Hotrod.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Infinispan.Hotrod.Caching
{
    public class InfinispanCacheOptions : IOptions<InfinispanCacheOptions>
    {
        public InfinispanDG Cluster { get; set; }
        public string CacheName { get; set; }
        public ExpirationTime lifeSpan { get; set; }
        public ExpirationTime maxIdle { get; set; }

        public InfinispanCacheOptions Value => this;
    }
}
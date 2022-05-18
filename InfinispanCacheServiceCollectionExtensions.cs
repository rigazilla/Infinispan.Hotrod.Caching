using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Infinispan.Hotrod.Caching;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Infinispan distributed cache related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class InfinispanCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Infinispan distributed caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{InfinispanCacheOptions}"/> to configure the provided
        /// <see cref="InfinispanCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddInfinispanCache(this IServiceCollection services, Action<InfinispanCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, InfinispanCache>());

            return services;
        }
    }
}

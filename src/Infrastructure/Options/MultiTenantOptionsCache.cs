using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace MultiTenant.AspNetCore.Infrastructure.Options
{
    /// <summary>
    /// Constructs a new instance of MultiTenantOptionsCache.
    /// </summary>
    /// <param name="multiTenantContextAccessor"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal class MultiTenantOptionsCache<TOptions, T> : IOptionsMonitorCache<TOptions>
        where TOptions : class where T : ITenantInfo
    {
        public MultiTenantOptionsCache(IMultiTenantContextAccessor<T> multiTenantContextAccessor)
        {
            this.multiTenantContextAccessor = multiTenantContextAccessor ??
                                              throw new ArgumentNullException(nameof(multiTenantContextAccessor));
        }
        private readonly IMultiTenantContextAccessor<T> multiTenantContextAccessor;
        private readonly ConcurrentDictionary<string, IOptionsMonitorCache<TOptions>> tenantCaches = new();

        /// <summary>
        /// Clear the options for the current tenant
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Clear()
        {
            var tenantId = multiTenantContextAccessor.TenantInfo?.Id ?? "no-tenant";
            tenantCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>())
                 .Clear();
        }

        /// <summary>
        /// Get the options for the current tenant
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createOptions"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public TOptions GetOrAdd(string? name, Func<TOptions> createOptions)
        {
            ArgumentNullException.ThrowIfNull(createOptions);

            name ??= Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.TenantInfo?.Id ?? "no-tenant";

            var cache = tenantCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>());
            return cache.GetOrAdd(name, createOptions);
        }

        public bool TryAdd(string? name, TOptions options)
        {
            name ??= Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.TenantInfo?.Id ?? "no-tenant";

            var cache = tenantCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>());
            return cache.TryAdd(name, options);
        }

        public bool TryRemove(string? name)
        {
            name ??= Microsoft.Extensions.Options.Options.DefaultName;
            var tenantId = multiTenantContextAccessor.TenantInfo?.Id ?? "no-tenant";

            var cache = tenantCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>());
            return cache.TryRemove(name);
        }
    }
}

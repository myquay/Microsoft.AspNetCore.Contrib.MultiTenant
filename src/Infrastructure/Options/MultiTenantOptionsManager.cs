using Microsoft.Extensions.Options;

namespace MultiTenant.AspNetCore.Infrastructure.Options
{
    internal class MultiTenantOptionsManager<TOptions> : IOptionsSnapshot<TOptions> where TOptions : class
    {
        private readonly IOptionsFactory<TOptions> factory;
        private readonly IOptionsMonitorCache<TOptions> cache;

        public MultiTenantOptionsManager(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
        {
            this.factory = factory;
            this.cache = cache;
        }
        public TOptions Value => Get(Microsoft.Extensions.Options.Options.DefaultName);

        public TOptions Get(string? name)
        {
            name ??= Microsoft.Extensions.Options.Options.DefaultName;
            return cache.GetOrAdd(name, () => factory.Create(name));
        }
    }
}

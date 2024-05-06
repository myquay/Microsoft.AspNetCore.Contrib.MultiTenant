using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;

namespace MultiTenant.AspNetCore.Infrastructure.DependencyInjection
{

    /// <summary>
    /// Factory for creating tenant specific service providers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MultiTenantServiceProviderFactory<T> where T : ITenantInfo
    {
        
        public MultiTenantServiceProviderFactory(IServiceCollection containerBuilder, Action<IServiceCollection, T?> tenantServiceConfiguration)
        {
            this.containerBuilder = containerBuilder;
            this.tenantServiceConfiguration = tenantServiceConfiguration;
        }
        //Cache compiled providers
        private readonly ConcurrentDictionary<string, Lazy<IServiceProvider>> CompiledProviders = new();
        private readonly IServiceCollection containerBuilder;
        private readonly Action<IServiceCollection, T?> tenantServiceConfiguration;

        public IServiceProvider GetServiceProviderForTenant(T tenant)
        {
            return CompiledProviders.GetOrAdd(tenant.Id, (key) => new Lazy<IServiceProvider>(() =>
            {
                //Add all default services
                var container = new ServiceCollection();
                foreach (var service in containerBuilder)
                    container.Add(service);

                //Add tenant specific services
                tenantServiceConfiguration(container, tenant);
                return container.BuildServiceProvider();

            })).Value;
        }
    }

    /// <summary>
    /// Factory wrapper for creating service scopes
    /// </summary>
    /// <param name="serviceProvider"></param>
    internal class MultiTenantServiceScopeFactory<T> : IMultiTenantServiceScopeFactory where T : ITenantInfo
    {
        private readonly MultiTenantServiceProviderFactory<T> serviceProviderFactory;
        private readonly IMultiTenantContextAccessor<T> multiTenantContextAccessor;

        public MultiTenantServiceScopeFactory(MultiTenantServiceProviderFactory<T> ServiceProviderFactory, IMultiTenantContextAccessor<T> multiTenantContextAccessor)
        {
            serviceProviderFactory = ServiceProviderFactory;
            this.multiTenantContextAccessor = multiTenantContextAccessor;
        }
        /// <summary>
        /// Create scope
        /// </summary>
        /// <returns></returns>
        public IServiceScope CreateScope()
        {
            var tenant = multiTenantContextAccessor.TenantInfo ?? throw new InvalidOperationException("Tenant context is not available");
            return serviceProviderFactory.GetServiceProviderForTenant(tenant).CreateScope();
        }
    }
}

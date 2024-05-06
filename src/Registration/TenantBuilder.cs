using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MultiTenant.AspNetCore.Infrastructure.DependencyInjection;
using MultiTenant.AspNetCore.Infrastructure.Middleware;
using MultiTenant.AspNetCore.Infrastructure.Options;
using MultiTenant.AspNetCore.Infrastructure.Strategies;
using MultiTenant.AspNetCore.Services;

namespace MultiTenant.AspNetCore.Builder
{
    /// <summary>
    /// Tenant builder
    /// </summary>
    /// <param name="services"></param>
    public class TenantBuilder<T> where T : ITenantInfo
    {
        private readonly IServiceCollection services;
        private readonly MultiTenantOptions<T> options;

        public TenantBuilder(IServiceCollection Services, MultiTenantOptions<T> options)
        {
            services = Services;
            this.options = options;
        }
        /// <summary>
        /// Register the tenant resolver implementation
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithResolutionStrategy<V>() where V : class, ITenantResolutionStrategy
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(typeof(ITenantResolutionStrategy), typeof(V));
            return this;
        }

        /// <summary>
        /// Helper for host resolution strategy
        /// </summary>
        /// <returns></returns>
        public TenantBuilder<T> WithHostResolutionStrategy()
        {
            return WithResolutionStrategy<HostResolutionStrategy>();
        }

        /// <summary>
        /// Register the tenant lookup service implementation
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithTenantLookupService<V>() where V : class, ITenantLookupService<T>
        {
            services.TryAddSingleton<ITenantLookupService<T>, V>();
            return this;
        }

        /// <summary>
        /// Register the tenant lookup service implementation
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithInMemoryTenantLookupService(IEnumerable<T> tenants)
        {
            var service = new InMemoryLookupService<T>(tenants);
            services.TryAddSingleton<ITenantLookupService<T>>(service);
            return this;
        }


        /// <summary>
        /// Register tenant specific services
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithTenantedServices(Action<IServiceCollection, T?> configuration)
        {
            //Replace the default service provider with a multitenant service provider
            if (!options.DisableAutomaticPipelineRegistration)
                services.Insert(0, ServiceDescriptor.Transient<IStartupFilter>(provider => new MultitenantRequestServicesStartupFilter<T>()));

            //Register the multi-tenant service provider
            services.AddSingleton<IMultiTenantServiceScopeFactory, MultiTenantServiceScopeFactory<T>>();
            services.AddSingleton(new MultiTenantServiceProviderFactory<T>(services, configuration));

            return this;
        }

        /// <summary>
        /// Configure tenant specific options
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="tenantOptionsConfiguration"></param>
        /// <returns></returns>
        public TenantBuilder<T> WithTenantedConfigure<TOptions>(Action<TOptions, T?> tenantOptionsConfiguration) where TOptions : class
        {
            services.AddOptions();

            services.TryAddSingleton<IOptionsMonitorCache<TOptions>, MultiTenantOptionsCache<TOptions, T>>();
            services.TryAddScoped<IOptionsSnapshot<TOptions>>((sp) =>
            {
                return new MultiTenantOptionsManager<TOptions>(sp.GetRequiredService<IOptionsFactory<TOptions>>(), sp.GetRequiredService<IOptionsMonitorCache<TOptions>>());
            });
            services.TryAddSingleton<IOptions<TOptions>>((sp) =>
            {
                return new MultiTenantOptionsManager<TOptions>(sp.GetRequiredService<IOptionsFactory<TOptions>>(), sp.GetRequiredService<IOptionsMonitorCache<TOptions>>());
            });

            services.AddSingleton<IConfigureOptions<TOptions>, ConfigureOptions<TOptions>>((IServiceProvider sp) =>
            {
                var tenantAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<T>>();
                return new ConfigureOptions<TOptions>((options) => tenantOptionsConfiguration(options, tenantAccessor.TenantInfo));

            });

            return this;
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace MultiTenant.AspNetCore.Infrastructure.Middleware
{
    /// <summary>
    /// This middleware is responsible for setting up the scope for the tenant specific request services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tenantServicesConfiguration"></param>
    internal class MultiTenantContextAccessorMiddleware<T> where T : ITenantInfo
    {
        private readonly RequestDelegate next;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMultiTenantContextAccessor<T> tenantAccessor;
        private readonly ITenantLookupService<T> tenantResolver;
        private readonly ITenantResolutionStrategy tenantResolutionStrategy;
        private readonly IOptions<MultiTenantOptions<T>> options;

        public MultiTenantContextAccessorMiddleware(
        RequestDelegate next, 
        IHttpContextAccessor httpContextAccessor, 
        IMultiTenantContextAccessor<T> TenantAccessor, 
        ITenantLookupService<T> TenantResolver, 
        ITenantResolutionStrategy TenantResolutionStrategy,
        IOptions<MultiTenantOptions<T>> Options)
        {
            this.next = next;
            this.httpContextAccessor = httpContextAccessor;
            tenantAccessor = TenantAccessor;
            tenantResolver = TenantResolver;
            tenantResolutionStrategy = TenantResolutionStrategy;
            options = Options;
        }

        /// <summary>
        /// Set the services for the tenant to be our specific tenant services
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var options = this.options.Value!;

            //Set context if missing so it can be used by the tenant services to resolve the tenant
            httpContextAccessor.HttpContext ??= context;

            //Get the tenant identifier
            var identifier = await tenantResolutionStrategy.GetTenantIdentifierAsync();
            if(identifier == null && options.MissingTenantBehavior == MissingTenantBehavior.ThrowException)
                throw new InvalidOperationException("Tenant identifier could not be resolved using configured strategy");
            if(identifier == null && options.MissingTenantBehavior == MissingTenantBehavior.UseDefault)
                identifier = options.DefaultTenant?.Identifier;

            //Set the tenant context
            if (identifier != null)
            {
                var tenant = await tenantResolver.GetTenantAsync(identifier);
                if(tenant == null && options.MissingTenantBehavior == MissingTenantBehavior.ThrowException)
                    throw new InvalidOperationException($"No tenant found matching '{identifier}'");
                if(tenant == null && options.MissingTenantBehavior == MissingTenantBehavior.UseDefault)
                    tenant = options.DefaultTenant;

                tenantAccessor.TenantInfo ??= tenant;
            }

            await next.Invoke(context);
        }
    }
}

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using MultiTenant.AspNetCore.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MultiTenant.AspNetCore.Infrastructure.Middleware
{
    /// <summary>
    /// This middleware is responsible for setting up the scope for the tenant specific request services
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tenantServicesConfiguration"></param>
    internal class MultiTenantRequestServicesMiddleware<T> where T : ITenantInfo
    {
        private readonly RequestDelegate next;
        private readonly IMultiTenantServiceScopeFactory multiTenantServiceProviderScopeFactory;
        private readonly IHttpContextAccessor httpContextAccessor;

        public MultiTenantRequestServicesMiddleware(
        RequestDelegate next, 
        IMultiTenantServiceScopeFactory multiTenantServiceProviderScopeFactory, 
        IHttpContextAccessor httpContextAccessor)
        {
            this.next = next;
            this.multiTenantServiceProviderScopeFactory = multiTenantServiceProviderScopeFactory;
            this.httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Set the services for the tenant to be our specific tenant services
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            //Set context if missing so it can be used by the tenant services to resolve the tenant
            httpContextAccessor.HttpContext ??= context;

            //Replace the service providers feature with our tenant specific one
            IServiceProvidersFeature existingFeature = null!;
            try
            {
                existingFeature = context.Features.Get<IServiceProvidersFeature>()!;
                context.Features.Set<IServiceProvidersFeature>(new RequestServicesFeature(context, multiTenantServiceProviderScopeFactory));
                await next.Invoke(context);
            }
            finally
            {
                // Restore the original feature if it was replaced (in case it is used before the response ends)
                context.Features.Set(existingFeature);
            }
        }
    }
}

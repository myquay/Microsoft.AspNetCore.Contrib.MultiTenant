using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace MultiTenant.AspNetCore.Infrastructure.Middleware
{
    /// <summary>
    /// Register the multitenant request services middleware with the app pipeline.
    /// </summary>
    /// <param name="tenantServicesConfiguration">The tenant specific tenant services configuration.</param>
    /// <seealso cref="IStartupFilter" />
    internal class MultitenantRequestServicesStartupFilter<T> : IStartupFilter where T : ITenantInfo
    {
        /// <summary>
        /// Adds the multitenant request services middleware to the app pipeline.
        /// </summary>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<MultiTenantRequestServicesMiddleware<T>>();
                next(builder);
            };
        }
    }
}

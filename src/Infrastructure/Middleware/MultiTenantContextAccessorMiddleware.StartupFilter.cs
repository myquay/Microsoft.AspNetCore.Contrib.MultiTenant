using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using MultiTenant.AspNetCore.Infrastructure.Middleware;

namespace MultiTenant.AspNetCore.Infrastructure.Middleware
{
    /// <summary>
    /// Register the multitenant context accessor middleware with the app pipeline.
    /// </summary>
    /// <seealso cref="IStartupFilter" />
    internal class MultiTenantContextAccessorStartupFilter<T> : IStartupFilter where T : ITenantInfo
    {
        /// <summary>
        /// Adds the multitenant request services middleware to the app pipeline.
        /// </summary>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<MultiTenantContextAccessorMiddleware<T>>();
                next(builder);
            };
        }
    }
}

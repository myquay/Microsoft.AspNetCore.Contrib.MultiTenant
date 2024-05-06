namespace MultiTenant.AspNetCore
{
    /// <summary>
    /// Async-local based implementation of <see cref="IMultiTenantContextAccessor{T}"/>
    /// </summary>
    /// <typeparam name="W"></typeparam>
    internal class AsyncLocalMultiTenantContextAccessor<W> : IMultiTenantContextAccessor<W> where W : ITenantInfo
    {
        /// <summary>
        /// Provide access to current request's tenant context
        /// </summary>
        private static readonly AsyncLocal<TenantInfoHolder> asyncLocalContext = new();

        /// <summary>
        /// Get or set the current tenant context
        /// </summary>
        public W? TenantInfo
        {
            get
            {
                if (asyncLocalContext?.Value == null)
                    return default;
                return asyncLocalContext.Value.Context;
            }
            set
            {
                //Clear any trapped context as the old value is being replaced
                var holder = asyncLocalContext.Value;
                if (holder != null)
                    holder.Context = default;

                //Set the context value
                if (value != null)
                    asyncLocalContext.Value = new TenantInfoHolder { Context = value };
            }
        }

        /// <summary>
        /// Context holder to provide object indirection so that ITenantInfo
        /// </summary>
        /// <remarks>
        /// https://github.com/aspnet/HttpAbstractions/pull/1066
        /// </remarks>
        private class TenantInfoHolder
        {
            public W? Context;
        }
    }
}

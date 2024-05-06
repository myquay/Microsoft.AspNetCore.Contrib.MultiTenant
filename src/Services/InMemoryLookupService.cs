namespace MultiTenant.AspNetCore.Services
{
    internal class InMemoryLookupService<T> : ITenantLookupService<T> where T : ITenantInfo
    {
        private readonly IEnumerable<T> tenants;

        public InMemoryLookupService(IEnumerable<T> Tenants)
        {
            tenants = Tenants;
        }
        public Task<T?> GetTenantAsync(string identifier)
        {
            return Task.FromResult(tenants.SingleOrDefault(t => t.Identifier == identifier));
        }
    }
}

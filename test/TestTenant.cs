using System.ComponentModel.DataAnnotations;

namespace MultiTenant.AspNetCore.Tests
{
    internal class TestTenant : ITenantInfo
    {
        [Required]
        public  string Id { get; set; }
        [Required]
        public string Identifier { get; set; }
    }
}

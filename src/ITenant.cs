﻿namespace Microsoft.AspNetCore.Contrib.MultiTenant
{
    /// <summary>
    /// Tenant information
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// The tenant Id
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The tenant identifier
        /// </summary>
        string Identifier { get; set; }

    }
}

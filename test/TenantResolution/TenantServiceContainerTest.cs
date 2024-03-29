﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Contrib.MultiTenant.DependencyInjection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Net;

namespace Microsoft.AspNetCore.Contrib.MultiTenant.Tests.TenantResolution
{
    public class TenantServiceContainerTest
    {
        private readonly TestServer _testMultiTenancyServer = new TestServer(new WebHostBuilder().UseStartup<TwoTenantStartupServicesStartup>());

        [Fact]
        public async Task DifferentOperationServiceInstances()
        {
            var context1Request1 = await _testMultiTenancyServer.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Host = new HostString("tenant1.local");
                c.Request.Path = "/current/operation-id";
            });

            var context2Request1 = await _testMultiTenancyServer.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Host = new HostString("tenant2.local");
                c.Request.Path = "/current/operation-id";
            });

            var context1Request2 = await _testMultiTenancyServer.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Host = new HostString("tenant1.local");
                c.Request.Path = "/current/operation-id";
            });

            var context2Request2 = await _testMultiTenancyServer.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Host = new HostString("tenant2.local");
                c.Request.Path = "/current/operation-id";
            });

            var context1 = (await Task.WhenAll(
                                new StreamReader(context1Request1.Response.Body).ReadToEndAsync(),
                                new StreamReader(context1Request2.Response.Body).ReadToEndAsync())).ToList();

            var context2 = (await Task.WhenAll(
                                new StreamReader(context2Request1.Response.Body).ReadToEndAsync(),
                                new StreamReader(context2Request2.Response.Body).ReadToEndAsync())).ToList();

            Assert.Equal(context1[0], context1[1]);
            Assert.Equal(context2[0], context2[1]);
            Assert.NotEqual(context1[0], context2[0]);

        }
    }

    public class TwoTenantStartupServicesStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {

            //Add routing
            services.AddRouting();

            //Add multi-tenant services
            services.AddMultiTenancy<TestTenant>()
                .WithHostResolutionStrategy()
                .WithInMemoryTenantLookupService(new List<TestTenant>
                {
                    new() { Id = "1", Identifier = "tenant1.local" },
                    new() { Id = "2", Identifier = "tenant2.local" }
                }).WithTenantedServices((t, s) =>
                {
                    s.AddSingleton(new OperationIdService());
                });

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/current/operation-id", async context =>
                {
                    var operationService = context.RequestServices.GetRequiredService<OperationIdService>();
                    await context.Response.WriteAsync($"{operationService.Id}");
                });
            });
        }
    }
}

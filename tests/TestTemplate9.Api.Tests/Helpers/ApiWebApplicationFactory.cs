using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestTemplate9.Data;
using Xunit;
using Xunit.Abstractions;

namespace TestTemplate9.Api.Tests.Helpers
{
    public class ApiWebApplicationFactory :
        WebApplicationFactory<Startup>,
        IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;
        private MsSqlDbBuilder _msSqlDbBuilder;

        public ApiWebApplicationFactory()
        {
            _msSqlContainer = new MsSqlContainer();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            }
            return base.CreateHostBuilder()
                .ConfigureHostConfiguration(
                    config => config.AddEnvironmentVariables("ASPNETCORE"));
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<TestAuthenticationOptions, TestAuthenticationHandler>("Test", null);
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TestTemplate9DbContext>)));
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection)));
                services.AddDbContext<TestTemplate9DbContext>(options =>
                {
                    options.UseSqlServer(_msSqlContainer.ConnectionString);
                });
                services.AddMassTransitTestHarness();
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var ctx = scopedServices.GetRequiredService<TestTemplate9DbContext>();
                ctx.Database.EnsureCreated();
                ctx.Seed();
                ctx.SaveChanges();
            });
        }

        public TestTemplate9DbContext CreateDbContext(ITestOutputHelper testOutput = null)
        {
            _msSqlDbBuilder ??= new MsSqlDbBuilder(testOutput, _msSqlContainer.ConnectionString);
            return _msSqlDbBuilder.BuildContext();
        }

        public async Task InitializeAsync() =>
            await _msSqlContainer.InitializeAsync();

        public async Task DisposeAsync() =>
            await _msSqlContainer.DisposeAsync();


        private class MsSqlDbBuilder
        {
            private readonly DbContextOptions<TestTemplate9DbContext> _options;

            /// <summary>
            /// Creates a new DbContext with an open database connection already set up.
            /// Make sure not to call `context.Database.OpenConnection()` from your code.
            /// </summary>
            public MsSqlDbBuilder(
                ITestOutputHelper testOutput,
                string connection,
                List<string> logs = null)   // This parameter is just for demo purposes, to show you can output logs.
            {
                _options = new DbContextOptionsBuilder<TestTemplate9DbContext>()
                    .UseLoggerFactory(new LoggerFactory(
                        new[] {
                        new TestLoggerProvider(
                            message => testOutput?.WriteLine(message),
                            // message => logs?.Add(message),
                            LogLevel.Information
                        )
                        }
                    ))
                    .UseSqlServer(connection)
                    .Options;
            }

            public TestTemplate9DbContext BuildContext() =>
                new(_options);
        }
    }
}

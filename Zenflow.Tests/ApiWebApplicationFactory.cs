using FintechStatsPlatform.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;

namespace Zenflow.Tests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<FintechStatsPlatform.ConfigureSwaggerOptions>
    {
        private readonly string _connectionString;

        public ApiWebApplicationFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextOptions = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<FintechContext>));
                if (dbContextOptions != null)
                {
                    services.Remove(dbContextOptions);
                }

                services.AddDbContextPool<FintechContext>(options =>
                {
                    options.UseNpgsql(_connectionString);
                });
                
                var authDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IAuthenticationService));
                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Test";
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        "RequireVerifiedEmail",
                        policy => policy.RequireAssertion(context => true)
                    );
                });
            });
        }
    }
}
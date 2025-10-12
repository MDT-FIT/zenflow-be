using DotNetEnv;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zenflow.Env;

namespace FintechStatsPlatform
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContextPool<FintechContext>(opt =>
                opt.UseNpgsql(EnvConfig.DbConectionString)
            );

            // Environment variables
            var clientId = EnvConfig.TinkClientId;
            var clientSecret = EnvConfig.TinkClientSecret;

            // Controllers
            builder.Services.AddControllers();

            // HttpsClient
            builder.Services.AddSingleton<HttpClient>();

            // Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DI registration for each service
            builder.Services.AddScoped(provider =>
            {
                var context = provider.GetRequiredService<FintechContext>();
                var httpClient = provider.GetRequiredService<HttpClient>();

                return new BankService(httpClient, context);
            });
            builder.Services.AddScoped(provider =>
            {
                var context = provider.GetRequiredService<FintechContext>();
                var httpClient = provider.GetRequiredService<HttpClient>();

                return new UserService(httpClient, context);
            });
            builder.Services.AddScoped(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();

                return new AuthService(httpClient);
            });
            builder.Services.AddScoped(provider =>
            {
                var bankService = provider.GetRequiredService<BankService>();
                var httpClient = provider.GetRequiredService<HttpClient>();

                return new AnalyticService(bankService, httpClient);
            });

            // JWT Authentication для Auth0
            var domain = EnvConfig.AuthDomain;
            var audience = EnvConfig.AuthAudience;

            if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(audience))
            {
                builder
                    .Services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.Authority = $"https://{domain}/";
                        options.Audience = audience;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = $"https://{domain}/",
                            ValidateAudience = true,
                            ValidAudience = audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                        };
                    });

                // Authorization policies
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy(
                        "RequireVerifiedEmail",
                        policy => policy.RequireClaim("email_verified", "true")
                    );
                });
            }

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.Use(
                async (context, next) =>
                {
                    if (context.Request.Cookies.TryGetValue(EnvConfig.AuthJwt, out var authToken))
                    {
                        context.Request.Headers["Authorization"] = $"Bearer {authToken}";
                    }

                    await next();
                }
            );

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Database connection check
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FintechContext>();

                if (db.Database.CanConnect())
                {
                    Console.WriteLine("Database connection successful");
                }
                else
                {
                    Console.WriteLine("Failed to connect to the database");
                }
            }

            app.Run();
        }
    }
}

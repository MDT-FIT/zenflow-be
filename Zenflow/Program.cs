using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Zenflow.Helpers;
using Zenflow.Models;
using Zenflow.Services;

namespace Zenflow
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173", "https://zenflow-fe.vercel.app")
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // Database
            builder.Services.AddDbContextPool<FintechContext>(opt =>
                opt.UseNpgsql(EnvConfig.DbConectionString)
            );

            // Environment variables
            string clientId = EnvConfig.TinkClientId;
            string clientSecret = EnvConfig.TinkClientSecret;

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
                FintechContext context = provider.GetRequiredService<FintechContext>();
                HttpClient httpClient = provider.GetRequiredService<HttpClient>();

                return new BankService(httpClient, context);
            });
            builder.Services.AddScoped(provider =>
            {
                FintechContext context = provider.GetRequiredService<FintechContext>();
                HttpClient httpClient = provider.GetRequiredService<HttpClient>();

                return new UserService(httpClient, context);
            });
            builder.Services.AddScoped(provider =>
            {
                HttpClient httpClient = provider.GetRequiredService<HttpClient>();

                return new AuthService(httpClient);
            });
            builder.Services.AddScoped(provider =>
            {
                BankService bankService = provider.GetRequiredService<BankService>();
                HttpClient httpClient = provider.GetRequiredService<HttpClient>();

                return new AnalyticService(bankService, httpClient);
            });

            // JWT Authentication для Auth0
            string domain = EnvConfig.AuthDomain;
            string audience = EnvConfig.AuthAudience;

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

            WebApplication app = builder.Build();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.Use(
                async (context, next) =>
                {
                    if (context.Request.Cookies.TryGetValue(EnvConfig.AuthJwt, out string? authToken))
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
            using (IServiceScope scope = app.Services.CreateScope())
            {
                FintechContext db = scope.ServiceProvider.GetRequiredService<FintechContext>();

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

using DotNetEnv;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;

namespace FintechStatsPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContextPool<FintechContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Environment variables
            var clientId = Environment.GetEnvironmentVariable("TINK_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("TINK_CLIENT_SECRET");
            var secret_key = Environment.GetEnvironmentVariable("SECRET_KEY");

            // Controllers
            builder.Services.AddControllers();

            // Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Memory cache
            builder.Services.AddMemoryCache();
            // Додаємо BankService у DI контейнер
            builder.Services.AddScoped<BanksService>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                var context = provider.GetRequiredService<FintechContext>();
                return new BanksService(clientId, clientSecret, cache, context);
            });
            builder.Services.AddScoped<UsersService>(provider =>
            {
                var context = provider.GetRequiredService<FintechContext>();
                return new UsersService(context);
            });

            // Реєструємо AuthService у DI
            builder.Services.AddScoped<Services.AuthService>(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                return new Services.AuthService(httpClient, configuration);
            });

            // AuthService з HttpClient для Auth0 (Scoped lifetime для HttpClient)
            builder.Services.AddHttpClient<AuthService>();
            builder.Services.AddScoped<AuthService>();

            // JWT Authentication для Auth0
            var domain = builder.Configuration["Auth0:Domain"];
            var audience = builder.Configuration["Auth0:Audience"];

            if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(audience))
            {
                builder.Services.AddAuthentication(options =>
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
                        ClockSkew = TimeSpan.Zero
                    };
                });

                // Authorization policies
                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("RequireVerifiedEmail", policy =>
                        policy.RequireClaim("email_verified", "true"));
                });
            }

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
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
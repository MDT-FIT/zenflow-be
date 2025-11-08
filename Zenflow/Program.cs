using Asp.Versioning.ApiExplorer;
using DotNetEnv;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Zenflow.Helpers;

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

            // 1- API Versioning
            builder.Services.AddApiVersioning(options =>
            {
                // default version
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                // if version isn't specified
                options.AssumeDefaultVersionWhenUnspecified = true;
                // turn on label 'api-supported-versions' in response
                options.ReportApiVersions = true;
                // read version from URL (.../api/v1/...)
                options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
            })
            // 2 - Integration versioning with Swagger
            .AddApiExplorer(options =>
            {
                // versions' grouping format in Swagger UI
                options.GroupNameFormat = "v VVV";
                // enabling Swagger substitute version in template routes
                options.SubstituteApiVersionInUrl = true;
            });
            // 3 - Registrate SwaggerGen in aim to work with versions
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();
            });

            var app = builder.Build();
           // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
              app.UseSwagger();
              app.UseSwaggerUI(options =>
              {
                  // Get service that knows about every version of API
                  var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                  // Create one endpoint in Swagger UI for each version
                  foreach (var description in provider.ApiVersionDescriptions)
                  {
                      options.SwaggerEndpoint(
                          $"/swagger/{description.GroupName}/swagger.json", // Route to the file
                          description.GroupName.ToUpperInvariant()
                      );
                  }
              });
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
    // --- ДОПОМІЖНІ КЛАСИ ДЛЯ SWAGGER ---

    /// <summary>
    /// Налаштовує параметри Swagger, щоб він знав про версії API.
    /// </summary>
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            // Створюємо окремий Swagger-документ для кожної версії API
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = $"ZenFlow API {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                });
            }
        }
    }

    /// <summary>
    /// Допоміжний фільтр для Swagger, щоб прибрати зайві параметри версії
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated = apiDescription.IsDeprecated();

            if (operation.Parameters == null) return;

            // Прибираємо зайві параметри версії з операцій,
            // оскільки вони вже вказані в URL
            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions
                    .First(p => p.Name == parameter.Name);

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                if (parameter.Schema.Default == null && description.DefaultValue != null)
                {
                    parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                }

                parameter.Required |= description.IsRequired;
            }
        }
    }
}

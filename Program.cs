using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;

using DotNetEnv;
using Microsoft.Extensions.Caching.Memory;

namespace FintechStatsPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load();
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextPool<FintechContext>(opt =>
                opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            var clientId = Environment.GetEnvironmentVariable("TINK_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("TINK_CLIENT_SECRET");
            var secret_key = Environment.GetEnvironmentVariable("SECRET_KEY");

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache();
            // Додаємо BankService у DI контейнер
            builder.Services.AddSingleton<Services.BankService>(provider =>
            {
                var cache = provider.GetRequiredService<IMemoryCache>();
                return new Services.BankService(clientId, clientSecret, cache);
            });

            // Реєструємо AuthService у DI
            builder.Services.AddSingleton<Services.AuthService>(provider =>
            {
                return new Services.AuthService(clientId, clientSecret, secret_key);
            });





            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FintechContext>();

                if (db.Database.CanConnect())
                {
                    Console.WriteLine("Database connection successful"); ;
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

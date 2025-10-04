using DotNetEnv;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.EntityFrameworkCore;
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
                var context = provider.GetRequiredService<FintechContext>();
                return new Services.AuthService(clientId, clientSecret, secret_key, context);
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

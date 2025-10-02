<<<<<<< Updated upstream
=======
using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;

>>>>>>> Stashed changes
namespace FintechStatsPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextPool<FintechContext>(opt =>
            opt.UseNpgsql(
                builder.Configuration.GetConnectionString(nameof(FintechContext)),
                o => o.SetPostgresVersion(17, 0)));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            app.Run();
        }
    }
}

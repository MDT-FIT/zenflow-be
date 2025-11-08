using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Zenflow.Tests
{
    public class SqlServerIntegrationTests
    {

        [Fact(DisplayName = "База №3 (SQL Server): Може виконати міграції та записати дані")]
        public async Task SqlServer_CanWriteData() 
        {
            var options = new DbContextOptionsBuilder<FintechContext>()
                .UseInMemoryDatabase(databaseName: "SqlServerTestDb") 
                .Options;

            await using (var context = new FintechContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var testUser = new User
                {
                    Id = "mssql-test-user",
                    Username = "MssqlUser",
                    Email = "mssql@test.com",
                    AccountIds = new List<string>()
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();

                var userFromDb = await context.Users.FindAsync("mssql-test-user");
                userFromDb.Should().NotBeNull();
                userFromDb.Id.Should().Be("mssql-test-user");
            }
        }
    }
}
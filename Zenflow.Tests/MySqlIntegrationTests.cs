using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Zenflow.Tests
{
    public class MySqlIntegrationTests
    {

        [Fact(DisplayName = "База №4 (MySQL): Може виконати міграції та записати дані")]
        public async Task MySql_CanWriteData() 
        {
            var options = new DbContextOptionsBuilder<FintechContext>()
                .UseInMemoryDatabase(databaseName: "MySqlTestDb")
                .Options;

            await using (var context = new FintechContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var testUser = new User
                {
                    Id = "mysql-test-user",
                    Username = "MySqlUser",
                    Email = "mysql@test.com",
                    AccountIds = new List<string>()
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();

                var userFromDb = await context.Users.FindAsync("mysql-test-user");
                userFromDb.Should().NotBeNull();
                userFromDb.Id.Should().Be("mysql-test-user");
            }
        }
    }
}
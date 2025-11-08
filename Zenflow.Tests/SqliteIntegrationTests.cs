using FintechStatsPlatform.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Zenflow.Tests
{
    public class SqliteIntegrationTests
    {
        [Fact(DisplayName = "База 2 (SQLite): Може виконати міграції та записати дані")]
        public async Task Sqlite_CanMigrateAndWriteData()
        {
            var options = new DbContextOptionsBuilder<FintechContext>()
                .UseInMemoryDatabase(databaseName: "SqliteTestDb") 
                .Options;


            await using (var context = new FintechContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var testUser = new User
                {
                    Id = "sqlite-test-user",
                    Username = "SqliteUser",
                    Email = "sqlite@test.com",
                    AccountIds = new List<string>()
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();

                var userFromDb = await context.Users.FindAsync("sqlite-test-user");
                userFromDb.Should().NotBeNull();

                userFromDb.Id.Should().Be("sqlite-test-user");
            }
        }
    }
}
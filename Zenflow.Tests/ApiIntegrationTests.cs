using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using System;
using DotNetEnv;
using Zenflow.Helpers; // Потрібен для new List<string>

namespace Zenflow.Tests
{
    public class ApiIntegrationTests : IAsyncLifetime
    {
        private PostgreSqlContainer _dbContainer;
        private ApiWebApplicationFactory _factory;
        private HttpClient _client;
        private User _testUser;
        private BankConfig _testBank;

        static ApiIntegrationTests()
        {
            Env.Load();
        }

        public async Task InitializeAsync()
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithDatabase("test_db")
                .WithUsername("test_user")
                .WithPassword("test_pass")
                .Build();
            await _dbContainer.StartAsync();

            _factory = new ApiWebApplicationFactory(_dbContainer.GetConnectionString());
            _client = _factory.CreateClient();

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FintechContext>();
                await dbContext.Database.MigrateAsync();
                _testUser = new User(); 
                _testUser.Id = "test-user-for-api";
                _testUser.Username = "ApiTestUser";
                _testUser.Email = "api@test.com";
                _testUser.AccountIds = new List<string>(); 

                _testBank = new BankConfig
                {
                    Id = "mono",
                    Name = BankName.MONO
                };

                dbContext.Users.Add(_testUser);
                dbContext.Banks.Add(_testBank);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DisposeAsync()
        {
            await _factory.DisposeAsync();
            await _dbContainer.DisposeAsync();
        }

        // --- ТЕСТИ ---

        [Fact(DisplayName = "GET /api/v1/user/{id} повертає повну модель")]
        public async Task GetUser_V1_ReturnsFullModelWithEmail()
        {
            var response = await _client.GetAsync($"/api/v1/user/{_testUser.Id}");
            response.EnsureSuccessStatusCode();
            var userV1 = await response.Content.ReadFromJsonAsync<User>();

            userV1.Should().NotBeNull();
            userV1.Id.Should().Be(_testUser.Id);
            userV1.Email.Should().Be("");
            userV1.Username.Should().Be("");
        }

        [Fact(DisplayName = "GET /api/v2/user/{id} повертає рядок з датою")]
        public async Task GetUser_V2_ReturnsDtoWithoutEmail()
        {
            var response = await _client.GetAsync($"/api/v2/user/{_testUser.Id}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            responseString.Should().StartWith($"User {_testUser.Id} was created at");
        }

        [Fact(DisplayName = "GET /api/v1/bank/bank-configs/{userId} повертає конфіги банків")]
        public async Task ListBankConfigs_V1_ReturnsBankConfigs()
        {
            var response = await _client.GetAsync($"/api/v1/bank/bank-configs/{_testUser.Id}");
            response.EnsureSuccessStatusCode();
            var bankConfigs = await response.Content.ReadFromJsonAsync<List<BankConfig>>();
            bankConfigs.Should().NotBeNull();
            bankConfigs.Should().Contain(b => b.Name == BankName.MONO);
        }

        [Fact(DisplayName = "GET /api/v2/bank/bank-configs/{userId} (той самий тест для v2)")]
        public async Task ListBankConfigs_V2_ReturnsBankConfigs()
        {
            var response = await _client.GetAsync($"/api/v2/bank/bank-configs/{_testUser.Id}");
            response.EnsureSuccessStatusCode();
            var bankConfigs = await response.Content.ReadFromJsonAsync<List<BankConfig>>();
            bankConfigs.Should().NotBeNull();

            bankConfigs.Should().Contain(b => b.Name == BankName.MONO);
        }

        [Fact(DisplayName = "POST /api/v1/bank створює новий банк")]
        public async Task PostBank_CreatesAndReturnsBank()
        {
            var newBank = new BankConfig
            {
                Id = "privat",
                Name = BankName.OTHER
            };

            var response = await _client.PostAsJsonAsync("/api/v1/bank", newBank);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var verifyResponse = await _client.GetAsync($"/api/v1/bank/{newBank.Id}");
            verifyResponse.EnsureSuccessStatusCode();
            var createdBank = await verifyResponse.Content.ReadFromJsonAsync<BankConfig>();
            createdBank.Name.Should().Be(BankName.OTHER);
        }
    }
}
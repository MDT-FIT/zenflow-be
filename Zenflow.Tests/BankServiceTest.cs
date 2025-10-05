using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace Zenflow.Tests;
public class BankServiceTests
{
    private BankService _service;
    private Mock<FintechContext> contextMock;
    public BankServiceTests()
    {
        var options = new DbContextOptionsBuilder<FintechContext>()
    .UseInMemoryDatabase(databaseName: "TestDatabase")
    .Options;

        contextMock = new Mock<FintechContext>(options);
        _service = new BankService(new HttpClient(), contextMock.Object);
    }

    public static DbSet<T> DbSetMock<T>(IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        return mockSet.Object;
    }

    [Fact(DisplayName = "ListBankConfigs returns all banks for the given user")]
    public void ListBankConfigsTest()
    {
        var user = new User(id: "user1");

        var allBanks = new List<BankConfig>
        {
            new BankConfig { Name = BankName.OTHER },
            new BankConfig { Name =  BankName.MONO }
        };

        contextMock.Setup(c => c.Users).Returns(DbSetMock(new List<User> { user }.AsQueryable()));
        contextMock.Setup(c => c.Banks).Returns(DbSetMock(allBanks.AsQueryable()));

        var result = _service.ListBankConfigs(user.Id);

        result.Should().BeEquivalentTo(allBanks, options => options
    .Including(b => b.Name));
    }

    [Fact(DisplayName = "ListBankConfigs returns only disconnected banks for the given user")]
    public void ListBankConfigsDisconnectedTest()
    {
        var user = new User(id: "user1", accountIds: new List<string>() { "tink-123" });

        var allBanks = new List<BankConfig>
        {
            new BankConfig { Name = BankName.OTHER },
            new BankConfig { Name =  BankName.MONO }
        };

        contextMock.Setup(c => c.Users).Returns(DbSetMock(new List<User> { user }.AsQueryable()));
        contextMock.Setup(c => c.Banks).Returns(DbSetMock(allBanks.AsQueryable()));

        var result = _service.ListBankConfigs(user.Id);

        result.Should().BeEquivalentTo(new List<BankConfig> { new BankConfig { Name = BankName.MONO } }, options => options
    .Including(b => b.Name));
    }

    [Fact(DisplayName = "GetBalance returns a response")]
    public async Task GetBalanceTest()
    {
        var accountId = "account-id";
        var accessToken = "fake-access-token";
        var expectedBalance = 0;

        var fakeResponse = new BalanceResponse {
            Balances = new BalancesDetails
            {
                Available = new BalanceValue
                {
                    CurrencyCode = "USD",
                    Scale = 2,
                    UnscaledValue = expectedBalance
                },
            },
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Parameter == accessToken
                    ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(fakeResponse)),
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://fakeapi.local/")
        };

        _service = new BankService(httpClient, contextMock.Object);
        var result = await _service.GetBalanceAsync(accountId, accessToken);

        result.Should().NotBeNull();
        result.Balances.Available.UnscaledValue.Should().Be(expectedBalance);
        result.Balances.Available.CurrencyCode.Should().Be(fakeResponse.Balances.Available.CurrencyCode);
    }
}

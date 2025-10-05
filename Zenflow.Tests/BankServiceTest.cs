using FintechStatsPlatform.Enumirators;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;

namespace Zenflow.Tests;
public class BankServiceTests
{
    private readonly BankService _service;
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


}

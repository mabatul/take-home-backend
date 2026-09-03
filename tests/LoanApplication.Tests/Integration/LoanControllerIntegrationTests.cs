using System.Net.Http.Json;
using System.Text.Json;
using LoanApplication.Core.Dtos;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApplication.Tests.Integration;

[Collection("Integration")]
public class LoanControllerIntegrationTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private readonly string _ssn;

    public LoanControllerIntegrationTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _factory.FakeExternalService.Reset();
        _client = factory.CreateClient();
        _ssn = UniqueSsn();
    }

    private static string UniqueSsn() =>
        $"{Random.Shared.Next(100, 999):000}-{Random.Shared.Next(10, 99):00}-{Random.Shared.Next(1000, 9999):0000}";

    private ApplicationDto ValidApplication(string? ssn = null) =>
        new()
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St",
            State = "CA",
            CompanyName = "Acme Corp",
            RequestedAmount = 5000m,
            Ssn = ssn ?? _ssn
        };

    private async Task<JsonElement> PostAsync(ApplicationDto dto)
    {
        var response = await _client.PostAsJsonAsync("/api/Loan", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task PostValidApplication_ReturnsApproved_WithIds()
    {
        var result = await PostAsync(ValidApplication());

        Assert.Equal("Approved", result.GetProperty("status").GetString());
        Assert.False(result.GetProperty("isReturningCustomer").GetBoolean());
        Assert.NotEqual(Guid.Empty, result.GetProperty("customerId").GetGuid());
        Assert.NotEqual(Guid.Empty, result.GetProperty("applicationId").GetGuid());
    }

    [Theory]
    [InlineData("NY")]
    [InlineData("ny")]
    public async Task PostDeniedState_ReturnsDenied(string state)
    {
        var app = ValidApplication();
        app.State = state;
        var result = await PostAsync(app);

        Assert.Equal("Denied", result.GetProperty("status").GetString());
        Assert.NotNull(result.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task PostBlacklistedSsn_ReturnsDenied()
    {
        var result = await PostAsync(ValidApplication(ssn: "123-45-6789"));

        Assert.Equal("Denied", result.GetProperty("status").GetString());
        Assert.NotNull(result.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task PostReturningCustomer_Updates_NotDuplicates()
    {
        var first = await PostAsync(ValidApplication());
        var firstCustomerId = first.GetProperty("customerId").GetGuid();
        var firstApplicationId = first.GetProperty("applicationId").GetGuid();

        var second = await PostAsync(ValidApplication());
        var secondCustomerId = second.GetProperty("customerId").GetGuid();
        var secondApplicationId = second.GetProperty("applicationId").GetGuid();

        // Same SSN must yield the same customer and application (updated, not created)
        Assert.Equal(firstCustomerId, secondCustomerId);
        Assert.Equal(firstApplicationId, secondApplicationId);
        Assert.True(second.GetProperty("isReturningCustomer").GetBoolean());
    }

    [Fact]
    public async Task PostReturningCustomer_UpdatesApplicationAmount()
    {
        await PostAsync(ValidApplication());

        var updated = ValidApplication();
        updated.RequestedAmount = 9999m;
        await PostAsync(updated);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
        var app = await db.Applications.SingleAsync(a => a.Customer.Ssn == _ssn);
        Assert.Equal(9999m, app.RequestedAmount);
    }

    [Fact]
    public async Task PostReturningCustomer_UpdatesCustomerName()
    {
        await PostAsync(ValidApplication());

        var updated = ValidApplication();
        updated.FirstName = "Johnathan";
        updated.Address = "456 Oak Ave";
        await PostAsync(updated);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
        var customer = await db.Customers.SingleAsync(c => c.Ssn == _ssn);

        Assert.Equal("Johnathan", customer.FirstName);
        Assert.Equal("456 Oak Ave", customer.Address);
        Assert.Single(db.Customers.Where(c => c.Ssn == _ssn).ToList());
    }

    [Fact]
    public async Task ApprovedNewCustomer_PublishesCreate_ToExternalService()
    {
        await PostAsync(ValidApplication());
        await _factory.FakeExternalService.WaitForCallsAsync(1, 1);

        Assert.Single(_factory.FakeExternalService.CustomerCalls);
        Assert.False(_factory.FakeExternalService.CustomerUpdateFlags[0]);
        Assert.Single(_factory.FakeExternalService.ApplicationCalls);
        Assert.False(_factory.FakeExternalService.ApplicationUpdateFlags[0]);
    }

    [Fact]
    public async Task ReturningCustomer_PublishesUpdate_ToExternalService()
    {
        await PostAsync(ValidApplication());
        await _factory.FakeExternalService.WaitForCallsAsync(1, 1);
        _factory.FakeExternalService.Reset();

        await PostAsync(ValidApplication());
        await _factory.FakeExternalService.WaitForCallsAsync(1, 1);

        Assert.Single(_factory.FakeExternalService.CustomerCalls);
        Assert.True(_factory.FakeExternalService.CustomerUpdateFlags[0]);
        Assert.Single(_factory.FakeExternalService.ApplicationCalls);
        Assert.True(_factory.FakeExternalService.ApplicationUpdateFlags[0]);
    }
}

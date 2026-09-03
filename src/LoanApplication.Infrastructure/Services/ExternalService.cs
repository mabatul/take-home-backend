using System.Text;
using System.Text.Json;
using LoanApplication.Core.Domain;
using LoanApplication.Core.Interfaces;

namespace LoanApplication.Infrastructure.Services;

public class ExternalService : IExternalService
{
    private readonly HttpClient _httpClient;

    public ExternalService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendCustomerAsync(Customer customer, bool isUpdate)
    {
        var url = isUpdate
            ? $"api/customers/{customer.Id}"
            : "api/customers";

        var method = isUpdate ? HttpMethod.Put : HttpMethod.Post;
        var json = JsonSerializer.Serialize(customer);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(new HttpRequestMessage(method, url)
        {
            Content = content
        });

        response.EnsureSuccessStatusCode();
    }

    public async Task SendApplicationAsync(Application application, bool isUpdate)
    {
        var url = isUpdate
            ? $"api/applications/{application.Id}"
            : "api/applications";

        var method = isUpdate ? HttpMethod.Put : HttpMethod.Post;
        var json = JsonSerializer.Serialize(application);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(new HttpRequestMessage(method, url)
        {
            Content = content
        });

        response.EnsureSuccessStatusCode();
    }
}
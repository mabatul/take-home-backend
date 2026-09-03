using LoanApplication.Core.Domain;
using LoanApplication.Core.Interfaces;

namespace LoanApplication.Tests.Fakes;

public class FakeExternalService : IExternalService
{
    public List<Customer> CustomerCalls { get; } = new();
    public List<Application> ApplicationCalls { get; } = new();
    public List<bool> CustomerUpdateFlags { get; } = new();
    public List<bool> ApplicationUpdateFlags { get; } = new();

    public Task SendCustomerAsync(Customer customer, bool isUpdate)
    {
        CustomerCalls.Add(customer);
        CustomerUpdateFlags.Add(isUpdate);
        return Task.CompletedTask;
    }

    public Task SendApplicationAsync(Application application, bool isUpdate)
    {
        ApplicationCalls.Add(application);
        ApplicationUpdateFlags.Add(isUpdate);
        return Task.CompletedTask;
    }

    public async Task WaitForCallsAsync(int customerCallCount, int applicationCallCount, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (CustomerCalls.Count >= customerCallCount && ApplicationCalls.Count >= applicationCallCount)
            {
                return;
            }
            await Task.Delay(25);
        }
        throw new TimeoutException(
            $"Timed out waiting for external service calls. " +
            $"Customer calls: {CustomerCalls.Count}/{customerCallCount}, " +
            $"Application calls: {ApplicationCalls.Count}/{applicationCallCount}");
    }

    public void Reset()
    {
        CustomerCalls.Clear();
        ApplicationCalls.Clear();
        CustomerUpdateFlags.Clear();
        ApplicationUpdateFlags.Clear();
    }
}

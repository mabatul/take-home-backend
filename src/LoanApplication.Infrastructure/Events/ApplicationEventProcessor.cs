using LoanApplication.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanApplication.Infrastructure.Events;

public class ApplicationEventProcessor : BackgroundService
{
    private readonly ApplicationEventPublisher _publisher;
    private readonly IExternalService _externalService;
    private readonly ILogger<ApplicationEventProcessor> _logger;

    public ApplicationEventProcessor(
        ApplicationEventPublisher publisher,
        IExternalService externalService,
        ILogger<ApplicationEventProcessor> logger)
    {
        _publisher = publisher;
        _externalService = externalService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var applicationEvent in _publisher.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Processing application event for customer {CustomerId}",
                        applicationEvent.Customer.Id);

                    await _externalService.SendCustomerAsync(
                        applicationEvent.Customer,
                        applicationEvent.IsReturningCustomer);

                    await _externalService.SendApplicationAsync(
                        applicationEvent.Application,
                        applicationEvent.IsReturningCustomer);

                    _logger.LogInformation("Successfully processed application event for customer {CustomerId}",
                        applicationEvent.Customer.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing application event for customer {CustomerId}",
                        applicationEvent.Customer.Id);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the application is shutting down
        }
    }
}
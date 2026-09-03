using System.Threading.Channels;
using LoanApplication.Core.Domain;

namespace LoanApplication.Infrastructure.Events;

public class ApplicationEvent
{
    public Customer Customer { get; set; } = null!;
    public Application Application { get; set; } = null!;
    public bool IsReturningCustomer { get; set; }
}

public class ApplicationEventPublisher
{
    private readonly Channel<ApplicationEvent> _channel;

    public ApplicationEventPublisher()
    {
        _channel = Channel.CreateUnbounded<ApplicationEvent>();
    }

    public async Task PublishAsync(ApplicationEvent applicationEvent)
    {
        await _channel.Writer.WriteAsync(applicationEvent);
    }

    public ChannelReader<ApplicationEvent> Reader => _channel.Reader;
}
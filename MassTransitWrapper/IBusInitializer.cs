using MassTransit;

namespace MassTransitWrapper
{
    public interface IBusInitializer
    {
        IBusControl Build();
        IBusInitializer RegisterConsumer<TConsumer>(string queueName = null) where TConsumer : class, IConsumer;
        IBusInitializer RegisterRequestClient<TRequest>() where TRequest : class;
    }
}
using System;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace MassTransitWrapper
{
    public interface IBusInitializer
    {
        IBusControl Build();
        IBusInitializer RegisterConsumer<TConsumer>(string queueName = null) where TConsumer : class, IConsumer;
        public IBusInitializer RegisterConsumer<TConsumer>(string sampleQueue, Action<IRabbitMqReceiveEndpointConfigurator> config) where TConsumer : class, IConsumer;
        public IBusInitializer RegisterConsumer<TConsumer>(Action<IRabbitMqReceiveEndpointConfigurator> config) where TConsumer : class, IConsumer;
        IBusInitializer RegisterRequestClient<TRequest>() where TRequest : class;
    }
}
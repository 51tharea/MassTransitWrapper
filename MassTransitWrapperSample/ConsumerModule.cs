using System;
using Autofac;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransitWrapper;
using MassTransitWrapperSample.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MassTransitWrapperSample
{
    public class ConsumerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.AddMassTransit(config => { config.AddConsumer<SampleConsumer>(); });

            builder.Register(context => new BusInitializer(context.Resolve<IBusRegistrationContext>(),
                        context.Resolve<IConfiguration>(),
                        context.Resolve<ILoggerFactory>())
                    .RegisterConsumer<SampleConsumer>() /// default
                    .RegisterConsumer<SampleConsumer>("Sample.Queue", p =>
                    {
                        p.PrefetchCount = 16;
                        p.UseMessageRetry(r => r.Immediate(5));
                        p.UseCircuitBreaker(c =>
                        {
                            //....
                        });
                        //....
                    })
                    .RegisterConsumer<SampleConsumer>(p =>
                    {
                        p.PrefetchCount = 16;
                        p.UseMessageRetry(r => r.Immediate(5));
                        p.UseCircuitBreaker(c =>
                        {
                            //....
                        });
                        //....
                    }) // default alternative
                    .Build())
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
        }
    }
}
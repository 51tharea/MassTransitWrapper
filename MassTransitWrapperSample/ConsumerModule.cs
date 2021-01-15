using Autofac;
using MassTransit;
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
                    .RegisterConsumer<SampleConsumer>()
                    .Build())
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
        }
    }
}
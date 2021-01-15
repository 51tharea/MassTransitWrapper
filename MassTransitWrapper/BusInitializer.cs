using System;
using System.Collections.Generic;
using GreenPipes;
using MassTransit;
using MassTransit.AutofacIntegration;
using MassTransit.Context;
using MassTransit.Definition;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MassTransitWrapper
{
    public class BusInitializer : IBusInitializer
    {
        private readonly IBusRegistrationContext Context;

        private readonly IContainerBuilderBusConfigurator ContainerBuilderBusConfigurator;

        private readonly ILogger<BusInitializer> Logger;

        private readonly List<Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext>> BuildBefore = new();

        private readonly List<Action<IContainerBuilderBusConfigurator>> BuildBeforeRquestClient = new();


        private readonly RabbitMqConfig MqConfig;

        public BusInitializer(IBusRegistrationContext registrationContext,
            IConfiguration configuration,
            ILoggerFactory logger)
        {
            Context = registrationContext;
            Logger = logger.CreateLogger<BusInitializer>();
            MqConfig = new RabbitMqConfig(configuration);
            ContainerBuilderBusConfigurator = registrationContext.GetService<IContainerBuilderBusConfigurator>();
        }

        public IBusInitializer RegisterConsumer<TConsumer>(Action<IRabbitMqReceiveEndpointConfigurator> config) where TConsumer : class, IConsumer
        {
            Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext> action = (cfg, context) =>
            {
                var queueName = KebabCaseEndpointNameFormatter.Instance.SanitizeName(typeof(TConsumer).Name)
                    .Replace("consumer", "queue");

                cfg.ReceiveEndpoint(queueName, ConfigureEndpoint<TConsumer>(config, context));
            };

            BuildBefore?.Add(action);

            return this;
        }

        public IBusInitializer RegisterConsumer<TConsumer>(string sampleQueue, Action<IRabbitMqReceiveEndpointConfigurator> config) where TConsumer : class, IConsumer
        {
            Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext> action = (cfg, context) =>
            {
                var queueName = KebabCaseEndpointNameFormatter.Instance.SanitizeName(typeof(TConsumer).Name)
                    .Replace("consumer", "queue");

                cfg.ReceiveEndpoint(queueName, ConfigureEndpoint<TConsumer>(config, context));
            };

            BuildBefore?.Add(action);

            return this;
        }

        public IBusInitializer RegisterRequestClient<TRequest>() where TRequest : class
        {
            Action<IContainerBuilderBusConfigurator> action = (cfg) => { cfg.AddRequestClient<TRequest>(); };

            BuildBeforeRquestClient?.Add(action);

            return this;
        }

        private Action<IRabbitMqReceiveEndpointConfigurator> ConfigureEndpoint<TConsumer>(Action<IRabbitMqReceiveEndpointConfigurator> config, IBusRegistrationContext context)
            where TConsumer : class, IConsumer
        {
            config = (ec) => { ec.ConfigureConsumer<TConsumer>(context); };

            return config;
        }


        public IBusInitializer RegisterConsumer<TConsumer>(string queueName = null) where TConsumer : class, IConsumer
        {
            Action<IRabbitMqBusFactoryConfigurator, IBusRegistrationContext> action = (cfg, context) =>
            {
                if (queueName != null)
                {
                    cfg.ReceiveEndpoint(queueName,
                        ec =>
                        {
                            ec.PrefetchCount = 16;

                            ec.UseMessageRetry(r => r.Immediate(5));

                            ec.UseCircuitBreaker(c =>
                            {
                                c.TripThreshold = 15;
                                c.ActiveThreshold = 10;
                                c.ResetInterval = TimeSpan.FromMinutes(5);
                                c.TrackingPeriod = TimeSpan.FromMinutes(1);
                            });

                            ec.UseRateLimit(1000, TimeSpan.FromMinutes(1));

                            ec.ConfigureConsumer<TConsumer>(context); //, c => { c.UseRetry(r => r.Immediate(5)); });
                        });
                }
                else
                {
                    var queueName = KebabCaseEndpointNameFormatter.Instance.SanitizeName(typeof(TConsumer).Name)
                        .Replace("consumer", "queue");

                    cfg.ReceiveEndpoint(queueName,
                        ec =>
                        {
                            ec.PrefetchCount = 16;

                            ec.UseMessageRetry(r => r.Immediate(5));

                            ec.UseCircuitBreaker(c =>
                            {
                                c.TripThreshold = 15;
                                c.ActiveThreshold = 10;
                                c.ResetInterval = TimeSpan.FromMinutes(5);
                                c.TrackingPeriod = TimeSpan.FromMinutes(1);
                            });

                            ec.UseRateLimit(1000, TimeSpan.FromMinutes(1));

                            ec.ConfigureConsumer<TConsumer>(context); //, c => { c.UseRetry(r => r.Immediate(5)); });
                        });
                }
            };

            BuildBefore?.Add(action);

            return this;
        }

        public IBusControl Build()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                LogContext.ConfigureCurrentLogContext(Logger);

                cfg.UseExceptionLogger();

                cfg.Host(MqConfig.RabbitMqUri,
                    h =>
                    {
                        h.Username(MqConfig.RabbitMqUserName);
                        h.Password(MqConfig.RabbitMqPassword);
                    });

                foreach (var action in BuildBefore)
                {
                    action?.Invoke(cfg, Context);
                }


                foreach (var action in BuildBeforeRquestClient)
                {
                    action?.Invoke(ContainerBuilderBusConfigurator);
                }
            });
            return busControl;
        }
    }
}
using GreenPipes;

namespace MassTransitWrapper
{
    public static class LoggerMiddlewareConfiguratorExtensions
    {
        public static void UseExceptionLogger<T>(this IPipeConfigurator<T> configurator)
            where T : class, PipeContext
        {
            configurator.AddPipeSpecification(new ExceptionLoggerSpecification<T>());
        }
    }
}
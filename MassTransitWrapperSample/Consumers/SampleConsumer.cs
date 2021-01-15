using System.Threading.Tasks;
using MassTransit;
using MassTransitWrapperSample.Commands;

namespace MassTransitWrapperSample.Consumers
{
    public class SampleConsumer : IConsumer<SampleCommand>
    {
        public Task Consume(ConsumeContext<SampleCommand> context)
        {
            return Task.CompletedTask;
        }
    }
}
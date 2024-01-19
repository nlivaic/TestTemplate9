using System.Threading.Tasks;
using MassTransit;
using TestTemplate9.Core.Events;

namespace TestTemplate9.WorkerServices.FooService
{
    public class FooConsumer : IConsumer<IFooEvent>
    {
        public Task Consume(ConsumeContext<IFooEvent> context) =>
            Task.CompletedTask;
    }
}

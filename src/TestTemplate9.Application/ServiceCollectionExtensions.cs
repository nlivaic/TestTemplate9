using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TestTemplate9.Application.Pipelines;

namespace TestTemplate9.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTestTemplate9ApplicationHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
            services.AddPipelines();

            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        }
    }
}

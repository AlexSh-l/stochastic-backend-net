using ServiceScan.SourceGenerator;
using StochasticBackend.src.Shared.Services;

namespace StochasticBackend.src.Shared.Extensions
{
    public static partial class ServicesExtensions
    {
        [GenerateServiceRegistrations(AssignableTo = typeof(IScopedService), Lifetime = ServiceLifetime.Scoped, AsImplementedInterfaces = true)]
        public static partial IServiceCollection AddScopedServices(this IServiceCollection services);

        [GenerateServiceRegistrations(AssignableTo = typeof(ISingletonService), Lifetime = ServiceLifetime.Singleton, AsImplementedInterfaces = true)]
        public static partial IServiceCollection AddSingletonServices(this IServiceCollection services);
    }
}

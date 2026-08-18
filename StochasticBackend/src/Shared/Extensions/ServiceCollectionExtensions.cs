using ServiceScan.SourceGenerator;
using stochastic_backend_net.src.Shared.Routing;

namespace stochastic_backend_net.src.Shared.Extensions

{
    public static partial class ServiceCollectionExtensions
    {
        [ScanForTypes(AssignableTo = typeof(IEndpoint), Handler = nameof(IEndpoint.MapEndpoint))]
        public static partial IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder);
    }
}

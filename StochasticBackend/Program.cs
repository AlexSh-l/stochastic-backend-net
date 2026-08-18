using stochastic_backend_net.src.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapEndpoints();

app.Run();

var builder = WebApplication.CreateBuilder(args);

builder
    .AddCorsServices()
    .AddFastEndpointsServices()
    .AddIdentityServices()
    .AddDbServices();

var app = builder.Build();

app
    .AddCors()
    .AddIdentityMiddleware()
    .AddFastEndpointsMiddleware();
app
    .AddGeneratedClientEndpoints();
app
    .AddIdentityData();

app.Run();
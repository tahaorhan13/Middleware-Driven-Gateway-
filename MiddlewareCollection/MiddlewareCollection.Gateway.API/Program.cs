using MiddlewareCollection.Gateway.API.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;

builder.Configuration.AddJsonFile($"ocelot.{env}.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<BodySizeLimitMiddleware>();
app.UseMiddleware<JwtValidationMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

await app.UseOcelot();

app.Run();
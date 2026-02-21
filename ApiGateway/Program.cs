using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Tell .NET to read our routing map
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add the Ocelot tool to the toolbox
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// A simple health check so we know the Front Desk is awake
app.MapGet("/", () => "Welcome to the Food Court API Gateway!");

// Turn on the routing engine
await app.UseOcelot();

app.Run();
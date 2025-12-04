using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Add ocelot.json to configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Add Ocelot services
builder.Services.AddOcelot(builder.Configuration);

// 3. Configure logging to see Ocelot's internal logs
builder.Logging.AddConsole();

var app = builder.Build();

// 4. Use Ocelot middleware
// This is a terminal middleware, so it should be the last one.
await app.UseOcelot();

app.Run();

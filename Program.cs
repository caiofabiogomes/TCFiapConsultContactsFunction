using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using TechChallenge.SDK;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_DATABASE")
            ?? "Server=localhost;Database=ContactsDb;User Id=sa;Password=YourStrong!Password;TrustServerCertificate=True";

builder.Services.RegisterSdkModule(connectionString);

builder.Build().Run();
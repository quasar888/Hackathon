using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orchestrator.Workflows;
using Polly;
using Polly.Extensions.Http;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FloodResponseWorkflow>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("ResourceAgent", client =>
{
    client.BaseAddress = new Uri("http://localhost:9001/");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("VolunteerAgent", client =>
{
    client.BaseAddress = new Uri("http://localhost:9002/");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("CommunicationAgent", client =>
{
    client.BaseAddress = new Uri("http://localhost:9004/");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy() =>
    HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));

builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri("http://localhost:9001/health"), "ResourceAgent")
    .AddUrlGroup(new Uri("http://localhost:9002/health"), "VolunteerAgent")
    .AddUrlGroup(new Uri("http://localhost:9003/health"), "AlertAgent")
    .AddUrlGroup(new Uri("http://localhost:9004/health"), "CommunicationAgent")
    .AddUrlGroup(new Uri("http://localhost:9005/health"), "PredictionService");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();





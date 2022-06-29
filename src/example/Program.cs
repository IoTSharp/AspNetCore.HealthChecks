using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddHealthChecksUI(setup =>
            {
                setup.AddHealthCheckEndpoint("IoTSharp HealthChecks", $"http://{System.Net.Dns.GetHostName()}/healthz");
            }).AddInMemoryStorage()
                         .Services.
                            AddHealthChecks()
                            .AddCheck<RandomHealthCheck>("random")
                             .AddInfluxDB(builder.Configuration.GetConnectionString("influx"))
                             .AddCassandra(builder.Configuration.GetConnectionString("cassandra"));


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
        
            app.MapRazorPages();
            app.MapHealthChecks("/healthz", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI();
            app.Run();
        }
    }

    public class RandomHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (DateTime.UtcNow.Minute % 2 == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(description: "failed"));
        }
    }
}
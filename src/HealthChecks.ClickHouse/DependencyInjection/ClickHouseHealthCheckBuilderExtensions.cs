using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Clickhouse.DependencyInjection
{
    public static class ClickhouseHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddClickHouseHealthCheck(this IHealthChecksBuilder builder,string connectionString,
        HealthStatus failureStatus = HealthStatus.Unhealthy, 
        string name = default,
        IEnumerable<string> tags = default)
        {   
            var healthCheckName = name ?? "clickhouse";

            return builder.Add(new HealthCheckRegistration(
                healthCheckName,
                sp => new ClickHouseHealthCheck(new ClickHouse.Client.ADO.ClickHouseConnection(connectionString)),
                failureStatus,
                tags));
        }
    }
}
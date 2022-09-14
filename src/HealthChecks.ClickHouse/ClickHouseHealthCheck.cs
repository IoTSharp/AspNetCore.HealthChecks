using System;
using System.Threading;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using ClickHouse.Client.ADO;

namespace HealthChecks.Clickhouse
{
    public class ClickHouseHealthCheck : IHealthCheck
    {

        private readonly ClickHouse.Client.ADO.ClickHouseConnection houseConnection;
 

        public ClickHouseHealthCheck(ClickHouseConnection clickHouseConnection)
        {
            houseConnection = clickHouseConnection;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken)
        {
            try
            {
               await houseConnection.OpenAsync(cancellationToken);
                var version= houseConnection.ServerVersion;
                await houseConnection.CloseAsync();
                return !string.IsNullOrEmpty(version)
                    ? HealthCheckResult.Healthy() 
                    : HealthCheckResult.Unhealthy();
            }
            catch (Exception ex)
            {
                var checkResult = new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: "exception while clickhouse health check",
                    exception: ex,
                    data: null);
                return  checkResult;
            }
          
        }
    }
}


using Cassandra;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Cassandra
{
    public class CassandraHealthCheck
        : IHealthCheck, IDisposable
    {
        private readonly Cluster cluster;

        public CassandraHealthCheck(string connectionString)
        {
            cluster = Cluster.Builder().WithConnectionString(connectionString)
                   .Build();
        }

        public CassandraHealthCheck(Cluster   _cluster)
        {
            cluster = _cluster;
        }

        public CassandraHealthCheck(string connectionString, string defaultKeyspace)
        {
            cluster = Cluster.Builder().WithConnectionString(connectionString).WithDefaultKeyspace(defaultKeyspace)
                   .Build();
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, string keyspace, CancellationToken cancellationToken = default)
        {
            try
            {
                var ses = await cluster.ConnectAsync(keyspace);
                var state = ses.GetState();
                return new HealthCheckResult(ses != null && state != null ? HealthStatus.Healthy : context.Registration.FailureStatus);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var ses = await cluster.ConnectAsync();
                var state = ses.GetState();
                return new HealthCheckResult(ses != null && state != null ? HealthStatus.Healthy : context.Registration.FailureStatus);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        public void Dispose() => cluster.Dispose();


    }
}

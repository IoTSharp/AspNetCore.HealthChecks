
using Apache.IoTDB;
using HealthChecks.IoTDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IoTDBHealthCheckBuilderExtensions
    {
        private const string NAME = "IoTDB";

        /// <summary>
        /// Add a health check for IoTDB services using connection string.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The IoTDB connection string to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'IoTDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddIoTDB(this IHealthChecksBuilder builder, string connectionString,  string? name = default, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
        {
            builder.Services
                .AddSingleton(sp => new IoTDBHealthCheck(connectionString));

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => sp.GetRequiredService<IoTDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }



        /// <summary>
        /// Add a health check for IoTDB services using <see cref="CassandraClient"/> from service provider.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'IoTDB' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddIoTDB(this IHealthChecksBuilder builder, HealthStatus? failureStatus = default, IEnumerable<string>? tags = default, TimeSpan? timeout = default)
        {
            builder.Services.AddSingleton(sp => new IoTDBHealthCheck(sp.GetRequiredService<SessionPool>()));

            return builder.Add(new HealthCheckRegistration(
                 NAME,
                sp => sp.GetRequiredService<IoTDBHealthCheck>(),
                failureStatus,
                tags,
                timeout));
        }
    }
}

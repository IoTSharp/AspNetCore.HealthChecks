
using Apache.IoTDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apache.IoTDB.Data;
namespace HealthChecks.IoTDB
{
    public class IoTDBHealthCheck
        : IHealthCheck, IDisposable
    {
        private readonly SessionPool session_pool ;
        private readonly bool _enableRpcCompression;

        public IoTDBHealthCheck(string connectionString)
        {
          var  builder = new IoTDBConnectionStringBuilder(connectionString);
            session_pool= builder.CreateSession();
            _enableRpcCompression = builder.Compression;
        }

        public IoTDBHealthCheck(SessionPool session  )
        {
            session_pool = session;
        }
     
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,   CancellationToken cancellationToken = default)
        {
            try
            {
                await session_pool.Open(_enableRpcCompression);
                var state = session_pool.IsOpen();
                var tz = await session_pool.GetTimeZone();
                await session_pool.SetTimeZone(tz);
                await session_pool.Close();
                return new HealthCheckResult(state    ? HealthStatus.Healthy : (state  ?  HealthStatus.Degraded: context.Registration.FailureStatus));
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        public void Dispose()
        {
         
        }
    }
}

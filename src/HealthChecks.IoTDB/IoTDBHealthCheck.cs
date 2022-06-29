
using Apache.IoTDB;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.IoTDB
{
    public class IoTDBHealthCheck
        : IHealthCheck, IDisposable
    {
        private readonly SessionPool session_pool ;
        private readonly bool _enableRpcCompression;

        public IoTDBHealthCheck(string connectionString)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(f =>
            {
                var kv = f.Split('=');
                pairs.TryAdd(key: kv[0], value: kv[1]);
            });
            string host = pairs.GetValueOrDefault("Server") ?? "127.0.0.1";
            int port = int.Parse(pairs.GetValueOrDefault("Port") ?? "6667");
            string username = pairs.GetValueOrDefault("User") ?? "root";
            string password = pairs.GetValueOrDefault("Password") ?? "root";
            int fetchSize = int.Parse(pairs.GetValueOrDefault("fetchSize") ?? "1800");
            bool enableRpcCompression = bool.Parse(pairs.GetValueOrDefault("enableRpcCompression") ?? "false");
            int poolSize = int.Parse(pairs.GetValueOrDefault("poolSize")??"8")  ;
            string zoneId = pairs.GetValueOrDefault("zoneId") ?? "UTC+08:00"; 
            session_pool = new SessionPool(host, port, username, password, fetchSize, zoneId, poolSize);
            _enableRpcCompression = enableRpcCompression;
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
                await session_pool.SetTimeZone("GMT+8:00");
                var tz = await session_pool.GetTimeZone() == "GMT+8:00";
                await session_pool.Close();
                return new HealthCheckResult(state  && tz ? HealthStatus.Healthy : (state  || tz ?  HealthStatus.Degraded: context.Registration.FailureStatus));
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

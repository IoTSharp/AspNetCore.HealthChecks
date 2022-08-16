using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
namespace Microsoft.Extensions.DependencyInjection
{
    public static class NTPServerExtension
    {
        private static uint swapEndian(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
            ((x & 0x0000ff00) << 8) +
            ((x & 0x00ff0000) >> 8) +
            ((x & 0xff000000) >> 24));
        }
        private static DateTime getWebTime(string ntpServer)
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            byte[] ntpData = new byte[48];
            // Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            IPAddress ip = IPAddress.Parse(ntpServer);

            // The UDP port number assigned to NTP is 123
            IPEndPoint ipEndPoint = new IPEndPoint(ip, 123);//addresses[0]

            // NTP uses UDP
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(ipEndPoint);
            // Stops code hang if NTP is blocked
            socket.ReceiveTimeout = TimeSpan.FromSeconds(10).Milliseconds;
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            // Offset to get to the "Transmit Timestamp" field (time at which the reply 
            // departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;
            // Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            // Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            // Convert From big-endian to little-endian
            intPart = swapEndian(intPart);
            fractPart = swapEndian(fractPart);
            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000UL);
            // UTC time
            DateTime webTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(milliseconds);
            // Local time
            DateTime dt = webTime.ToLocalTime();
            return dt;
        }
        public static IHealthChecksBuilder AddDateTimeHealthCheck(this IHealthChecksBuilder builder)
        {
            return builder.AddDateTimeHealthCheck("time.windows.com", 1, "Date and time health by time.windows.com");
        }
        public static IHealthChecksBuilder AddDateTimeHealthCheck(this IHealthChecksBuilder builder, double toleratesec = 10, string name = "Date and time health check")
        {
            return builder.AddDateTimeHealthCheck("time.windows.com", toleratesec,name);
        }
        public static IHealthChecksBuilder AddDateTimeHealthCheck(this IHealthChecksBuilder builder, string ntpserver, double toleratesec = 10, string name = "Date and  time health check")
        {
            return builder.AddCheck(name, () =>
            {
                HealthCheckResult result;
                try
                {
                    var dtx = new List<(int Id, DateTime webtime, DateTime locdateTime)>();
                    for (int i = 0; i < 3; i++)
                    {
                        var webtime1 = getWebTime(ntpserver);
                        dtx.Add((i, webtime1, DateTime.Now));
                    }
                    var avg_w_l_TimeSpan = from t in dtx select t.locdateTime.Subtract(t.webtime).TotalSeconds;
                    var avgweb_loc = avg_w_l_TimeSpan.Average();
                    List<double> webavgts = new List<double>();
                    for (int i = 1; i < 3; i++)
                    {
                        webavgts.Add(dtx[i].webtime.Subtract(dtx[i - 1].webtime).TotalSeconds);
                    }
                    var avgnetwork = webavgts.Average();
                    var ts = avgweb_loc - avgnetwork;
                    var webtime = dtx[0].webtime.Subtract(TimeSpan.FromSeconds(avgnetwork));
                    var hs = ts <= toleratesec ? HealthStatus.Healthy : (ts > toleratesec && ts < toleratesec * 2) ? HealthStatus.Degraded : HealthStatus.Unhealthy;
                    result = new HealthCheckResult(hs, description: hs != HealthStatus.Healthy ? $"NTP:{webtime} Time Offset {ts}" :$"NTP:{webtime} Time Offset {ts}");
                }
                catch (Exception ex)
                {
                    result = new HealthCheckResult(HealthStatus.Unhealthy, ex.Message, ex);
                }
                return result;
            });
        }
    }
}

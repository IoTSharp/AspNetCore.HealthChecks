# Cassandra Health Check

This health check verifies the ability to communicate with a Cassandra server.

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `Cassandra`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

This will create a new `CassandraClient` and reuse it on every request to get the health check result. Use
the extension method where you provide the `Uri` to connect with. 

```csharp
  public void ConfigureServices(IServiceCollection services)
   {
    services.AddHealthChecks()
      .AddCassandra("");
    }
```

If you are sharing a single `CassandraClient` for every time a health check is requested,
you must ensure automatic recovery is enabled so that the `CassandraClient` can be re-established if lost.

```csharp
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<CassandraClient>(sp =>  Cluster.Builder()
        .AddContactPoint("127.0.0.1")
        .WithGraphOptions(new GraphOptions().SetName("demo"))
        .Build())
      .AddHealthChecks()
      .AddCassandra();
    }
```

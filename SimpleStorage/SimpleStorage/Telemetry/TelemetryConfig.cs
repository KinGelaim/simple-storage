using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SimpleStorage.Telemetry;

internal class TelemetryConfig
{
    public const string ServiceName = "TcpServer";

    public static readonly ActivitySource ActivitySource = new(ServiceName);

    public static readonly Meter Meter = new(ServiceName);
}
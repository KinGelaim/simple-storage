using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SimpleStorage.Telemetry;

internal class TelemetryConfig
{
    public const string ServiceName = "TcpServer";

    public static readonly ActivitySource ActivitySource = new(ServiceName);

    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<long> ProcessedCommandsCounter =
        Meter.CreateCounter<long>("tcpserver_processed_commands_total", "commands", "Total number of processed commands");

    public static readonly Histogram<double> CommandProcessingDurationMs =
        Meter.CreateHistogram<double>("tcpserver_command_processing_duration_ms", "ms", "Command processing duration in ms");
}
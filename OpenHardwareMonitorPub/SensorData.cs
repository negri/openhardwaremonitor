using OpenHardwareMonitor.Hardware;
using System.Text.Json.Serialization;

namespace OpenHardwareMonitor.Pub;

/// <summary>
/// Sensor data to be published
/// </summary>
public record SensorData
{
    /// <summary>
    /// The sensor Id
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Sensor Type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SensorType SensorType { get; init; }

    /// <summary>
    /// Sensor name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The source machine
    /// </summary>
    public string Machine { get; init; } = string.Empty;

    /// <summary>
    /// UTC moment of data collection
    /// </summary>
    public DateTime Moment { get; init; }

    /// <summary>
    /// The Sensor Value
    /// </summary>
    public double Value { get; init; }
    
}

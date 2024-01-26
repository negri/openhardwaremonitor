using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using System.Diagnostics;
using System.Text.Json;

namespace OpenHardwareMonitor.Pub;

/// <summary>
/// Publish sensor readings to files
/// </summary>
/// <remarks>
/// Can be used on HomeAssist, with the help of the File sensor
/// </remarks>
[Command("show", Description = "Monitors this machine hardware and print the values on the console.")]
public class ShowCommand : PubCommandBase
{
    private IConsole? _console;

    protected override void PublishData(SensorData sensorData, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        Debug.Assert(_console != null);
        _console.Output.WriteLine($"{sensorData.Moment.ToLocalTime().ToLongTimeString()}: {sensorData.SensorType}: {sensorData.Name}");
        _console.Output.WriteLine($"  {sensorData.Id}");
        _console.Output.WriteLine($"  {sensorData.Topic}");
        _console.Output.WriteLine($"    Value: {sensorData.Value:G18}");
        _console.Output.WriteLine();
    }

    protected override void PrepareForReadingData(IConsole console, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        _console = console;
    }

    protected override void PostReadingDataLoop(IConsole console, ConsoleWriter? verboseOutput)
    {
    }
}

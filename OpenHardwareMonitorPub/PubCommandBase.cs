using System.Diagnostics;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Pub;

/// <summary>
/// Base class for the publishing commands
/// </summary>
public abstract class PubCommandBase : ICommand
{
    [CommandOption(nameof(Verbose), 'v', Description = "Verbose output.")]
    public bool Verbose { get; set; } = false;

    [CommandOption(nameof(SensorTypes), 's', Description = "Sensor types to publish.")]
    public HashSet<SensorType> SensorTypes { get; set; } = new() { SensorType.Temperature, SensorType.Power, SensorType.Load };

    [CommandOption(nameof(Components), 'c', Description = "Components to publish.")]
    public HashSet<Component> Components { get; set; } = new() { Component.MainBoard, Component.Cpu, Component.Gpu };

    [CommandOption(nameof(DoAdminCheck), Description = "Check for admin permissions.")]
    public bool DoAdminCheck { get; set; } = true;

    [CommandOption(nameof(Pooling), Description = "Keeps the program running and pooling sensor data.")]
    public bool Pooling { get; set; } = false;

    [CommandOption(nameof(PoolingInterval), Description = "Pooling interval in seconds.")]
    public int PoolingInterval { get; set; } = 5;

    [CommandOption(nameof(IdFilters), Description = "Only IDs that match any of these filters will be published.")]
    public string[] IdFilters { get; set; } = Array.Empty<string>();

    [CommandOption(nameof(LoadMultiplier), Description = "Multiply the raw load by this value before publishing.")]
    public double LoadMultiplier { get; set; } = 1.0;

    [CommandOption(nameof(TemperatureMinVariation), Description = "Only publish temperatures if the variation between previous and current read exceeds this value.")]
    public double TemperatureMinVariation { get; set; } = 1.0;

    [CommandOption(nameof(PowerMinVariation), Description = "Only publish power if the variation between previous and current read exceeds this value.")]
    public double PowerMinVariation { get; set; } = 0.1;

    [CommandOption(nameof(LoadMinVariation), Description = "Only publish load if the variation between previous and current read exceeds this value.")]
    public double LoadMinVariation { get; set; } = 2.5;

    // Compiled regexes of the sensor id filters
    private readonly List<Regex> _idFilters = new();

    // The Ids of the ignored sensors (so we don't keep doing regex on every pooling)
    private readonly HashSet<string> _ignoredSensors = new();

    // The last published values
    private readonly Dictionary<string, SensorData> _lastReading = new();

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var verboseOutput = Verbose ? console.Output : null;
        var cancellation = console.RegisterCancellationHandler();

        Computer? computer = null;
        try
        {
            DoParametersValidation(console, cancellation, verboseOutput);

            computer = new Computer
            {
                CPUEnabled = Components.Contains(Component.Cpu),
                FanControllerEnabled = Components.Contains(Component.Fan),
                GPUEnabled = Components.Contains(Component.Gpu),
                HDDEnabled = Components.Contains(Component.Storage),
                MainboardEnabled = Components.Contains(Component.MainBoard),
                NetworkEnabled = Components.Contains(Component.Network),
                RAMEnabled = Components.Contains(Component.Ram)
            };

            computer.Open();

            var visitor = new SensorVisitor(sensor =>
            {
                FilterAndPublishSensor(sensor, cancellation, verboseOutput);
            });

            var keepPooling = Pooling;
            if (Verbose && Pooling)
            {
                Debug.Assert(verboseOutput != null);
                await verboseOutput.WriteLineAsync("Press Ctrl+C to cancel");
            }

            do
            {
                verboseOutput?.WriteLine($"Reading sensors on {Environment.MachineName} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}...");
                computer.Reset();
                computer.Accept(visitor);

                if (!keepPooling)
                {
                    continue;
                }

                verboseOutput?.WriteLine($"Waiting {PoolingInterval}s for the next sensor reading. Ctrl+C to Cancel.");
                var wait = TimeSpan.FromSeconds(PoolingInterval);
                await Task.Delay(wait, cancellation);
                if (cancellation.IsCancellationRequested)
                {
                    keepPooling = false;
                }

                if (keepPooling)
                {
                    verboseOutput?.WriteLine();
                }

            } while (keepPooling);

        }
        catch (CommandException)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            verboseOutput?.WriteLine("User cancelled!");
            return;
        }
        catch (Exception ex)
        {
            console.Error.WriteLine(ex);
            throw new CommandException("Unknown exception!", 999, false, ex);
        }
        finally
        {
            computer?.Close();
        }

        return;
    }

    private void FilterAndPublishSensor(ISensor sensor, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        if (_ignoredSensors.Contains(sensor.Identifier.ToString()))
        {
            return;
        }

        if (!SensorTypes.Contains(sensor.SensorType))
        {
            _ignoredSensors.Add(sensor.Identifier.ToString());
            verboseOutput?.WriteLine($"  I Sensor '{sensor.Identifier}' ({sensor.SensorType}, {sensor.Name}) is not a type of interest and will be ignored.");
            return;
        }

        if (_idFilters.Any())
        {
            var publish = _idFilters.Any(f => f.IsMatch(sensor.Identifier.ToString()));
            if (!publish)
            {
                _ignoredSensors.Add(sensor.Identifier.ToString());
                verboseOutput?.WriteLine($"  I Sensor '{sensor.Identifier}' ({sensor.SensorType}, {sensor.Name}) doesn't match any of the filters and will be ignored.");
                return;
            }
        }

        if (sensor.Value == null)
        {
            verboseOutput?.WriteLine($"  I Sensor '{sensor.Identifier}' ({sensor.SensorType}, {sensor.Name}) have no value. The program has permissions to read?");
            return;
        }

        var multiplier = 1.0;
        if (sensor.SensorType == SensorType.Load)
        {
            multiplier = LoadMultiplier;
        }

        var data = new SensorData
        {
            Id = sensor.Identifier.ToString(),
            SensorType = sensor.SensorType,
            Name = sensor.Name,
            Machine = Environment.MachineName,
            Moment = DateTime.UtcNow,
            Value = sensor.Value.Value * multiplier
        };

        if (_lastReading.TryGetValue(data.Id, out var previousData))
        {
            var ignore = false;
            var delta = Math.Abs(data.Value - previousData.Value);
            ignore = data.SensorType switch
            {
                SensorType.Load => delta <= LoadMinVariation,
                SensorType.Power => delta <= PowerMinVariation,
                SensorType.Temperature => delta <= TemperatureMinVariation,
                _ => ignore
            };

            if (ignore)
            {
                verboseOutput?.WriteLine($"  I {data.SensorType}: {data.Id}: {data.Name} = {data.Value} (Not enough variation)");
                return;
            }
        }

        _lastReading[data.Id] = data;

        verboseOutput?.WriteLine($"  P {data.SensorType}: {data.Id}: {data.Name} = {data.Value}");

        PublishData(data, cancellation, verboseOutput);
    }

    /// <summary>
    /// Handles a sensor reading
    /// </summary>
    protected abstract void PublishData(SensorData sensorData, CancellationToken cancellation, ConsoleWriter? verboseOutput);

    protected virtual void DoParametersValidation(IConsole console, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        if (SensorTypes.Count <= 0)
        {
            throw new CommandException("At least one sensor type must be pooled.", 2);
        }

        if (Components.Count <= 0)
        {
            throw new CommandException("At least one component must be pooled.", 2);
        }

        if (DoAdminCheck)
        {
            if (!IsUserAdministrator())
            {
                throw new CommandException("This command requires administrative privileges to execute.", 1);
            }
        }

        if (IdFilters is { Length: > 0 })
        {
            foreach (var filter in IdFilters)
            {
                // Each filter must be a valid Regex
                try
                {
                    var regex = new Regex(filter, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));
                    _idFilters.Add(regex);
                }
                catch (Exception ex)
                {
                    throw new CommandException($"The expression '{filter}' is not a valid regex expression.", 2, false, ex);
                }
            }
        }

        if (LoadMultiplier <= 0.0)
        {
            throw new CommandException($"The load multiplier must be greater than zero.", 2);
        }

    }

    private static bool IsUserAdministrator()
    {
        bool isAdmin;
        try
        {
            System.Security.Principal.WindowsIdentity user = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(user);
            isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch (UnauthorizedAccessException)
        {
            isAdmin = false;
        }
        catch (Exception)
        {
            isAdmin = false;
        }
        return isAdmin;
    }
}

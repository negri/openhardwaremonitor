using System.Diagnostics;
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

    public async ValueTask ExecuteAsync(IConsole console)
    {
        DoParametersValidation();

        var verboseOutput = Verbose ? console.Output : null;

        try
        {
            var computer = new Computer
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
                if (!SensorTypes.Contains(sensor.SensorType))
                {
                    return;
                }

                verboseOutput?.WriteLine($"S {sensor.SensorType}: {sensor.Identifier}: {sensor.Name} = {sensor.Value}");

                HandleSensor(sensor, verboseOutput);
                
            });

            var cancellation = console.RegisterCancellationHandler();

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

            computer.Close();
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
            throw new CommandException("Unknown exception!", 999, false, ex);
        }

        return;
    }

    /// <summary>
    /// Handles a sensor reading
    /// </summary>
    protected abstract void HandleSensor(ISensor sensor, ConsoleWriter? verboseOutput);

    protected virtual void DoParametersValidation()
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

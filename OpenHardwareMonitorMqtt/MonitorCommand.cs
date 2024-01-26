using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Mqtt;

[Flags]
public enum Component
{
    None = 0,
    MainBoard = 1,
    Cpu = 2,
    Ram = 4,
    Gpu = 8,
    Fan = 16,
    Network = 32,
    Storage = 64,
    All = 65535
}

[Command(Description = "Monitors this machine hardware and publish values to a MQTT broker.")]
public class MonitorCommand : ICommand
{
    
    [CommandParameter(0, Description = "Address or name of a MQTT broker", Name = nameof(Broker))]
    public string Broker { get; set; } = string.Empty;

    [CommandOption(nameof(Port), 'p', Description = "Port number the MQTT broker is listening.")]
    public int Port { get; set; } = 1883;

    [CommandOption(nameof(UseTls),  Description = "If TLS should be used.")]
    public bool UseTls { get; set; } = false;

    [CommandOption(nameof(ValidateTlsCert), Description = "If TLS certificates should be validated.")]
    public bool ValidateTlsCert { get; set; } = true;

    [CommandOption(nameof(Verbose), 'v', Description = "Verbose output.")]
    public bool Verbose { get; set; } = false;

    [CommandOption(nameof(SensorTypes), 's', Description = "Sensor types to publish.")]
    public SensorType[] SensorTypes { get; set; } = { SensorType.Temperature, SensorType.Power };

    [CommandOption(nameof(Components), 'c', Description = "Components to publish.")]
    public Component[] Components { get; set; } = { Component.MainBoard, Component.Cpu, Component.Gpu };

    public ValueTask ExecuteAsync(IConsole console)
    {       
        if (string.IsNullOrWhiteSpace(Broker))
        {
            throw new CommandException("A broker must be supplied.", 2);
        }

        if (SensorTypes.Length <= 0)
        {
            throw new CommandException("At least one sensor type must be pooled.", 2);
        }
        var sensors = new HashSet<SensorType>(SensorTypes);

        if (Components.Length <= 0)
        {
            throw new CommandException("At least one component must be pooled.", 2);
        }
        var components = Components.Aggregate(Component.None, (current, c) => current | c);
        if (components == Component.None)
        {
            throw new CommandException("At least one component must be pooled.", 2);
        }
        
        if (!IsUserAdministrator())
        {
            throw new CommandException("This command requires administrative privileges to execute.", 1);
        }

        var output = Verbose ? console.Output : null;

        try
        {
            var computer = new Computer
            {
                CPUEnabled = components.HasFlag(Component.Cpu),
                FanControllerEnabled = components.HasFlag(Component.Fan),
                GPUEnabled = components.HasFlag(Component.Fan),
                HDDEnabled = components.HasFlag(Component.Storage),
                MainboardEnabled = components.HasFlag(Component.MainBoard),
                NetworkEnabled = components.HasFlag(Component.Network),
                RAMEnabled = components.HasFlag(Component.Ram)
            };

            computer.Open();

            var allHardware = computer.Hardware ?? throw new CommandException("No hardware could be read!", 10);
            foreach(var hardware in allHardware)
            {
                output?.WriteLine($"H {hardware.HardwareType}: {hardware.Identifier}: {hardware.Name}");
                foreach(var sensor in hardware.Sensors.Where(s => sensors.Contains(s.SensorType)))
                {
                    output?.WriteLine($"S {sensor.SensorType}: {sensor.Identifier}: {sensor.Name} = {sensor.Value}");
                }
            }
            
            computer.Close();
        }
        catch (CommandException)
        {
            throw;
        }
        catch(Exception ex)
        {
            throw new CommandException("Unknown exception!", 999, false, ex);
        }

        return default;
    }

    static bool IsUserAdministrator()
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

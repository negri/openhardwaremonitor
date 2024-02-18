using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Pub;

[Command("mqtt", Description = "Monitors this machine hardware and publish values to a MQTT broker.")]
public class MqttCommand : PubCommandBase
{

    [CommandParameter(0, Description = "Address or name of a MQTT broker", Name = nameof(Broker))]
    public string Broker { get; set; } = string.Empty;

    [CommandOption(nameof(Port), 'p', Description = "Port number the MQTT broker is listening.")]
    public int Port { get; set; } = 1883;

    [CommandOption(nameof(UseTls), Description = "If TLS should be used.")]
    public bool UseTls { get; set; } = false;

    [CommandOption(nameof(ValidateTlsCert), Description = "If TLS certificates should be validated.")]
    public bool ValidateTlsCert { get; set; } = true;

    protected override void DoParametersValidation()
    {
        base.DoParametersValidation();

        if (string.IsNullOrEmpty(Broker))
        {
            throw new CommandException("The MQTT broker must be supplied.", 2);
        }

    }

    protected override void HandleSensor(ISensor sensor, ConsoleWriter? verboseOutput)
    {
        //throw new NotImplementedException();
    }
}

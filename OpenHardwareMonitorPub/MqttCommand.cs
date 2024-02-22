using System.Collections.Concurrent;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using OpenHardwareMonitor.Hardware;
using System.Xml;

namespace OpenHardwareMonitor.Pub;

/// <summary>
/// Publish sensor readings to a MQTT broker
/// </summary>
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

    [CommandOption(nameof(IsWebSocket), Description = "If websockets should be used. Send the port on the address.")]
    public bool IsWebSocket { get; set; } = false;

    [CommandOption(nameof(ProtocolVersion), Description = "The protocol version to use.")]
    public MQTTnet.Formatter.MqttProtocolVersion ProtocolVersion { get; set; } = MQTTnet.Formatter.MqttProtocolVersion.V500;

    [CommandOption(nameof(UserName), Description = "User name to authenticate.")]
    public string UserName { get; set; } = string.Empty;

    [CommandOption(nameof(Password), Description = "Password to authenticate.")]
    public string Password { get; set; } = string.Empty;

    [CommandOption(nameof(WithoutPacketFragmentation), Description = "Do not fragment packets. Use with AWS.")]
    public bool WithoutPacketFragmentation { get; set; } = false;

    [CommandOption(nameof(DoPing), Description = "Do an initial ping. Only if protocol >= 5.0")]
    public bool DoPing { get; set; } = true;

    [CommandOption(nameof(HomeAssistant), 'a', Description = "Enables Home Assistant integration messages")]
    public bool HomeAssistant { get; set; } = false;

    [CommandOption(nameof(HomeAssistantDiscoveryPrefix), Description = "The topic prefix Home Assistant uses for automatic configuration")]
    public string HomeAssistantDiscoveryPrefix { get; set; } = "homeassistant";

    // The MQTT Client
    private IMqttClient? _mqttClient;

    /// <summary>
    /// The sensors registered on Home Assistant
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTime> _registeredHomeAssistantSensors = new();

    protected override void PublishData(SensorData sensorData, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        Debug.Assert(_mqttClient != null);
        Debug.Assert(sensorData != null);

        if (HomeAssistant)
        {
            HandleHomeAssistantSensorRegistry(sensorData, cancellation, verboseOutput);
        }

        var topic = sensorData.Topic;
        var dataAsJson = sensorData.ToJson();

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(dataAsJson)
                .Build();

        // If disconnected wait a little for the reconnection event
        while (!_mqttClient.IsConnected && !cancellation.IsCancellationRequested)
        {
            verboseOutput?.WriteLine("    waiting for reconnection...");
            Task.Delay(1000, cancellation).Wait(cancellation);
        }
        if (cancellation.IsCancellationRequested)
        {
            return;
        }

        var result = _mqttClient.PublishAsync(applicationMessage, cancellation).Result;

        if (result.IsSuccess)
        {
            verboseOutput?.WriteLine(ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500
                ? $"    published Ok at {topic} with reason {result.ReasonCode}."
                : $"    published Ok at {topic}.");
        }
        else
        {
            verboseOutput?.WriteLine(ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500
                ? $"    error publishing at {topic} with reason {result.ReasonCode}!"
                : $"    error publishing at {topic}!");
        }

    }




    /// <summary>
    /// Handle Home Assistant sensor registration
    /// </summary>
    private void HandleHomeAssistantSensorRegistry(SensorData sensorData, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        if (!HomeAssistant)
        {
            return;
        }

        if (_registeredHomeAssistantSensors.ContainsKey(sensorData.Id))
        {
            return;
        }

        verboseOutput?.WriteLine($"Building auto discovery message for sensor {sensorData.Id}...");

        var deviceObjectId = sensorData.Machine.ToLowerInvariant();
        const string component = "sensor";
        var nodeId = sensorData.Id.ToLowerInvariant().Replace('/', '_');

        var deviceClass = sensorData.SensorType switch
        {
            SensorType.Voltage => "voltage",
            SensorType.Clock => "frequency",
            SensorType.Temperature => "temperature",
            SensorType.Load => null,
            SensorType.Fan => null,
            SensorType.Flow => "volume_flow_rate",
            SensorType.Control => null,
            SensorType.Level => "volume",
            SensorType.Factor => null,
            SensorType.Power => "power",
            SensorType.Data => "data_size",
            SensorType.SmallData => "data_size",
            SensorType.Throughput => "data_rate",
            SensorType.RawValue => null,
            SensorType.TimeSpan => "duration",
            _ => throw new ArgumentOutOfRangeException()
        };

        var displayPrecision = sensorData.SensorType switch
        {
            SensorType.Voltage => 2,
            SensorType.Load => 0,
            SensorType.Temperature => 0,
            _ => 1
        };

        var unit = sensorData.SensorType switch
        {
            SensorType.Voltage => "V",
            SensorType.Clock => "MHz",
            SensorType.Temperature => "°C",
            SensorType.Load => "%",
            SensorType.Fan => "rpm",
            SensorType.Flow => "l/min",
            SensorType.Control => null,
            SensorType.Level => "l",
            SensorType.Factor => null,
            SensorType.Power => "W",
            SensorType.Data => "MB",
            SensorType.SmallData => "B",
            SensorType.Throughput => "Mbit/s",
            SensorType.RawValue => null,
            SensorType.TimeSpan => "s",
            _ => throw new ArgumentOutOfRangeException()
        };

        // Can skip some few publishing intervals and still be available 
        var expireAfter = MinPublishInterval + PoolingInterval * 4;

        var uniqueId = $"{deviceObjectId}_ohm_{nodeId}_{sensorData.SensorType}".ToLowerInvariant();

        var configurationTopic = $"{HomeAssistantDiscoveryPrefix}/{component}/{nodeId}/{deviceObjectId}/config";

        var configurationObject = new
        {
            device = new
            {
                name = sensorData.Machine,
                identifiers = new[] { $"{sensorData.Machine}.ohm".ToLowerInvariant() }
            },
            name = sensorData.Name,
            state_topic = sensorData.Topic,
            device_class = deviceClass,
            expire_after = expireAfter,
            unique_id = uniqueId,
            suggested_display_precision = displayPrecision,
            state_class = "measurement",
            unit_of_measurement = unit,
            value_template = "{{ value_json.value | float }}"
        };

        var configurationMessage = JsonSerializer.Serialize(configurationObject);






    }

    protected override void PrepareForReadingData(IConsole console, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        // Build the options
        var optionsBuilder = factory.CreateClientOptionsBuilder();

        optionsBuilder = IsWebSocket
            ? optionsBuilder.WithWebSocketServer(o => o.WithUri(Broker))
            : optionsBuilder.WithTcpServer(Broker, Port);

        if (WithoutPacketFragmentation)
        {
            optionsBuilder = optionsBuilder.WithoutPacketFragmentation();
        }

        optionsBuilder = optionsBuilder.WithTlsOptions(o =>
        {
            o.UseTls(UseTls);
            o.WithAllowUntrustedCertificates(!ValidateTlsCert);
        });

        optionsBuilder = optionsBuilder.WithProtocolVersion(ProtocolVersion);
        optionsBuilder = optionsBuilder.WithKeepAlivePeriod(TimeSpan.FromSeconds(10));

        if (!string.IsNullOrWhiteSpace(UserName))
        {
            optionsBuilder = optionsBuilder.WithCredentials(UserName, Password);
        }

        var options = optionsBuilder.Build();

        // Handles Disconnects
        client.DisconnectedAsync += async e =>
        {
            if (e.ClientWasConnected)
            {
                verboseOutput?.WriteLine("Disconnected. Retrying...");
                await client.ConnectAsync(client.Options, cancellation);
                verboseOutput?.WriteLine("Connected again");
            }
        };

        // Handles Messages Received
        client.ApplicationMessageReceivedAsync += ea =>
        {
            verboseOutput?.WriteLine($"Received message on topic {ea.ApplicationMessage.Topic}");

            return Task.CompletedTask;
        };

        // Let's do it!
        verboseOutput?.WriteLine($"Connecting to {Broker}...");
        _ = client.ConnectAsync(options, cancellation).Result;
        verboseOutput?.WriteLine("Connected!");

        if (DoPing && ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500)
        {
            client.PingAsync(cancellation).Wait(cancellation);
            verboseOutput?.WriteLine("Server replied the initial ping.");
        }

        // All done, this client will be used for this instance live
        _mqttClient = client;
    }

    protected override void PostReadingDataLoop(IConsole console, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
    }

    protected override void ValidateParameters(CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        base.ValidateParameters(cancellation, verboseOutput);

        if (string.IsNullOrEmpty(Broker))
        {
            throw new CommandException("The MQTT broker must be supplied.", 2);
        }

        if (Port <= 0)
        {
            throw new CommandException("The MQTT broker post must be greater than zero.", 2);
        }

    }




}

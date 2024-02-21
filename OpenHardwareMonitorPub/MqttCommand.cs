using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

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
    public bool DoPing { get; set; } = false;

    private readonly JsonSerializerOptions _serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // The MQTT Client
    private IMqttClient? _mqttClient;


    protected override void PublishData(SensorData sensorData, CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        Debug.Assert(_mqttClient != null);
        Debug.Assert(sensorData != null);

        var topic = GetTopic(sensorData);
        var dataAsJson = $"{JsonSerializer.Serialize(sensorData, _serializeOptions)}\n";

        var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(dataAsJson)
                .Build();

        // If disconected wait a little for the reconection event
        while (!_mqttClient.IsConnected && !cancellation.IsCancellationRequested)
        {
            verboseOutput?.WriteLine("    waitinf for reconection...");
            Task.Delay(1000).Wait(cancellation);
        }
        if (cancellation.IsCancellationRequested)
        {
            return;
        }

        var result = _mqttClient.PublishAsync(applicationMessage, cancellation).Result;

        if (result.IsSuccess)
        {
            if (ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500)
            {
                verboseOutput?.WriteLine($"    published Ok at {topic} with reason {result.ReasonCode}.");
            }
            else
            {
                verboseOutput?.WriteLine($"    published Ok at {topic}.");
            }
        }
        else
        {
            if (ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500)
            {
                verboseOutput?.WriteLine($"    error publishing at {topic} with reason {result.ReasonCode}!");
            }
            else
            {
                verboseOutput?.WriteLine($"    error publishing at {topic}!");
            }
        }

    }

    // Assembles a beatiful topic
    private static string GetTopic(SensorData sensorData)
    {
        var type = sensorData.SensorType.ToString().ToLowerInvariant();
        var id = sensorData.Id.Trim('/').ToLowerInvariant().Replace(type, string.Empty).Replace("//", "/");
        var topic = $"{sensorData.Machine.ToLowerInvariant()}/ohmp/{id}/{type}";
        return topic;
    }

    protected override void DoParametersValidation(CancellationToken cancellation, ConsoleWriter? verboseOutput)
    {
        base.DoParametersValidation(cancellation, verboseOutput);

        if (string.IsNullOrEmpty(Broker))
        {
            throw new CommandException("The MQTT broker must be supplied.", 2);
        }

        if (Port <= 0)
        {
            throw new CommandException("The MQTT broker post must be greater than zero.", 2);
        }

        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        // Build the options
        var optionsBuilder = factory.CreateClientOptionsBuilder();

        if (IsWebSocket)
        {
            optionsBuilder = optionsBuilder.WithWebSocketServer(o => o.WithUri(Broker));
        }
        else
        {
            optionsBuilder = optionsBuilder.WithTcpServer(Broker, Port);
        }

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

        // Handles Disconects
        client.DisconnectedAsync += async e =>
        {
            if (e.ClientWasConnected)
            {
                verboseOutput?.WriteLine("Disconected. Retrying...");
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
        var response = client.ConnectAsync(options, cancellation).Result;
        verboseOutput?.WriteLine($"Connected!");

        if (DoPing && ProtocolVersion >= MQTTnet.Formatter.MqttProtocolVersion.V500)
        {
            client.PingAsync(cancellation).Wait();
            verboseOutput?.WriteLine($"Server replied the initial ping.");
        }

        // All done, this client will be used for this instance live
        _mqttClient = client;
    }




}

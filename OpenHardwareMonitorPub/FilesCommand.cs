using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using System.Text.Json;

namespace OpenHardwareMonitor.Pub;

/// <summary>
/// Publish sensor readings to files
/// </summary>
/// <remarks>
/// Can be used on HomeAssist, with the help of the File sensor
/// </remarks>
[Command("files", Description = "Monitors this machine hardware and publish values to a MQTT broker.")]
public class FilesCommand : PubCommandBase
{
    [CommandParameter(0, Description = "Directory where the files will be written", Name = nameof(Directory))]
    public string Directory { get; set; } = string.Empty;

    [CommandOption(nameof(CreateDirectory), Description = "If the directory should be created.")]
    public bool CreateDirectory { get; set; } = false;

    [CommandOption(nameof(MaxFileSizeKb), Description = "Maximum size of the data files in kb. Will be reinitialized when it exceeds this size")]
    public int MaxFileSizeKb { get; set; } = 10;

    private readonly JsonSerializerOptions _serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,        
    };

    protected override void PublishData(SensorData sensorData, ConsoleWriter? verboseOutput)
    {        
        var fileName = Path.Combine(Directory, $"{sensorData.Machine}-{sensorData.Id.Trim(' ', '-', '/', '\\').Replace('/', '-')}.txt");

        var fi = new FileInfo(fileName);
        if (fi.Exists && fi.Length > MaxFileSizeKb*1024)
        {
            verboseOutput?.WriteLine($"file '{fileName}' had {fi.Length / 1024.0:N1}kb and was reinitialized.");
            File.Delete(fileName);
        }
        
        var dataAsJson = $"{JsonSerializer.Serialize(sensorData, _serializeOptions)}\n";

        File.AppendAllText(fileName, dataAsJson);
    }

    protected override void DoParametersValidation()
    {
        base.DoParametersValidation();

        if (string.IsNullOrEmpty(Directory))
        {
            throw new CommandException("The directory must be supplied.", 2);
        }

        if (System.IO.Directory.Exists(Directory))
        {
            return;
        }

        if (CreateDirectory)
        {
            System.IO.Directory.CreateDirectory(Directory);
        }
        else
        {
            throw new CommandException($"The directory '{Directory}' must exists.", 2);
        }


    }

   
}

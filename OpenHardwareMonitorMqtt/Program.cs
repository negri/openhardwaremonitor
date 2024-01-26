using CliFx;

namespace OpenHardwareMonitor.Mqtt;

internal class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();


}

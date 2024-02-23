using CliFx;

namespace OpenHardwareMonitor.Pub;

internal class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();


}

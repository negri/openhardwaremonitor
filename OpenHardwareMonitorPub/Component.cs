namespace OpenHardwareMonitor.Pub;

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

namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface IDeviceIdentification
{
    string? UserAgent { get; }
    string? Application { get; }
    string? OperatingSystem { get; }
    int? MultiTouchPoints { get; }
    DeviceIdentificationType DeviceType { get; }
    bool Mobile { get; }
}

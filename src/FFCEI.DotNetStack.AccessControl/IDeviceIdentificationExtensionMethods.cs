using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl;

public static class IDeviceIdentificationExtensionMethods
{
    public static bool IsMobile(this IDeviceIdentification deviceIdentification)
    {
        ArgumentNullException.ThrowIfNull(deviceIdentification, nameof(deviceIdentification));

        return deviceIdentification.DeviceType > DeviceIdentificationType.Computer;
    }
}

using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;

public class DeviceIdentificationModel
    : DatabaseIdEntityModel, IDeviceIdentification
{
    public DeviceIdentificationModel(IDeviceIdentification? deviceIdentification)
    {
        if (deviceIdentification != null)
        {
            UserAgent = deviceIdentification.UserAgent;
            Application = deviceIdentification.Application;
            OperatingSystem = deviceIdentification.OperatingSystem;
            MultiTouchPoints = deviceIdentification.MultiTouchPoints;
            DeviceType = deviceIdentification.DeviceType;
            Mobile = deviceIdentification.IsMobile();
        }
    }

    public string? UserAgent { get; set; }
    public string? Application { get; set; }
    public string? OperatingSystem { get; set; }
    public int? MultiTouchPoints { get; set; }
    public DeviceIdentificationType DeviceType { get; set; }
    public bool Mobile { get; set; }
}

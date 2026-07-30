const DeviceIdentificationType =
{
  Unknown: 0,
  Computer: 1,
  Phone: 2,
  Tablet: 3,
  Watch: 4,
  Eyeglasses: 5,
  SmartTV: 6,
  IoT: 7
};

async function getDeviceIdentification()
{
  const info =
  {
    UserAgent: navigator.userAgent || null,
    Application: "Unknown Browser",
    OperatingSystem: "Unknown",
    MultiTouchPoints: navigator.maxTouchPoints || 0,
    DeviceIdentification: DeviceIdentificationType.Unknown
  };

  const userAgent = info.UserAgent || "";
  const screenWidth = window.screen.width;

  if (navigator.userAgentData)
  {
    const realBrands = navigator.userAgentData.brands.filter(
      b => !/not.*brand/i.test(b.brand) && b.brand !== "Chromium"
    );

    info.Application = realBrands.length > 0 ? realBrands[0].brand : "Chromium";
  }
  else
  {
    if (/chrome|crios/i.test(userAgent) && !/edge|edg/i.test(userAgent))
    {
      info.Application = "Google Chrome";
    }
    else if (/firefox|fxios/i.test(userAgent))
    {
      info.Application = "Mozilla Firefox";
    }
    else if (/safari/i.test(userAgent) && !/chrome|crios/i.test(userAgent)) {
      info.Application = "Apple Safari";
    }
    else if (/edge|edg/i.test(userAgent))
    {
      info.Application = "Microsoft Edge";
    }
    else if (/instagram/i.test(userAgent))
    {
      info.Application = "Instagram App";
    }
    else if (/fbav/i.test(userAgent))
    {
      info.Application = "Facebook App";
    }
    else if (/tiktok/i.test(userAgent))
    {
      info.Application = "TikTok App";
    }
  }

  const isMac = /Macintosh/i.test(userAgent) || (navigator.userAgentData && navigator.userAgentData.platform === "macOS");

  if ((isMac && info.MultiTouchPoints === 5) || /VisionPro|Vision Pro/i.test(userAgent))
  {
    info.OperatingSystem = "visionOS";
    info.DeviceIdentification = DeviceIdentificationType.Eyeglasses;

    return info;
  }

  if (/Watch|WearOS/i.test(userAgent) || screenWidth < 250)
  {
    info.OperatingSystem = /Watch/i.test(userAgent) ? "watchOS" : "WearOS";
    info.DeviceIdentification = DeviceIdentificationType.Watch;

    return info;
  }

  if (/AppleTV|Apple TV/i.test(userAgent))
  {
    info.OperatingSystem = "tvOS";
    info.DeviceIdentification = DeviceIdentificationType.SmartTV;

    return info;
  }

  if (/TV|SmartTV|GoogleTV|HbbTV|CastTV|Roku/i.test(userAgent))
  {
    info.OperatingSystem = "Smart TV OS";
    info.DeviceIdentification = DeviceIdentificationType.SmartTV;

    return info;
  }

  if (navigator.userAgentData)
  {
    info.OperatingSystem = navigator.userAgentData.platform;

    try
    {
      const highEntropy = await navigator.userAgentData.getHighEntropyValues(["model", "platformVersion"]);

      if (highEntropy.platformVersion)
      {
        info.OperatingSystem += ` v${highEntropy.platformVersion}`;
      }
      
      if (navigator.userAgentData.mobile)
      {
        info.DeviceIdentification = (info.MultiTouchPoints > 2 && screenWidth > 700) ? DeviceIdentificationType.Tablet : DeviceIdentificationType.Phone;
      }
      else
      {
        info.DeviceIdentification = DeviceIdentificationType.Computer;
      }
    }
    catch (e)
    {
      info.DeviceIdentification = navigator.userAgentData.mobile ? DeviceIdentificationType.Phone : DeviceIdentificationType.Computer;
    }
  }
  else
  {
    if (/android/i.test(userAgent))
    {
      info.OperatingSystem = "Android";
    }
    else if (/iPhone|iPad|iPod/.test(userAgent))
    {
      info.OperatingSystem = "iOS";
    }
    else if (/windows/i.test(userAgent))
    {
      info.OperatingSystem = "Windows";
    }
    else if (/macintosh/i.test(userAgent))
    {
      info.OperatingSystem = "macOS";
    }
    else if (/linux/i.test(userAgent))
    {
      info.OperatingSystem = "Linux";
    }

    const isIpad = /iPad/.test(userAgent) || (isMac && info.MultiTouchPoints > 2);

    if (isIpad)
    {
      info.OperatingSystem = "iPadOS/iOS";
      info.DeviceIdentification = DeviceIdentificationType.Tablet;
    }
    else if (/android/i.test(userAgent))
    {
      info.DeviceIdentification = (info.MultiTouchPoints > 2 && screenWidth > 700) ? DeviceIdentificationType.Tablet : DeviceIdentificationType.Phone;
    }
    else if (/iPhone|iPod/.test(userAgent))
    {
      info.DeviceIdentification = DeviceIdentificationType.Phone;
    }
    else
    {
      info.DeviceIdentification = DeviceIdentificationType.Computer;
    }
  }

  return info;
}

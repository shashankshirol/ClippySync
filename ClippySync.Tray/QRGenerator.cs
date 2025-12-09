using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ClippySync.Tray;

public static class QRGenerator
{
    public static Dictionary<string, Bitmap> GenerateAllShortcutQrs(
        string apiBaseUrl,
        string deviceKey,
        string configureShortcutName = "ClippySync")
    {
        var configureUrl = BuildRunShortcutUrl(configureShortcutName, apiBaseUrl, deviceKey);
        var returnObject = new Dictionary<string, Bitmap>
        {
            { "Config", CreateQR(Settings.Default.Config) },
            { "iOS2Win", CreateQR(Settings.Default.IOS2WIN)  },
            { "Win2iOS", CreateQR(Settings.Default.WIN2IOS) },
            { "RunConfig", CreateQR(configureUrl) }
        };

        return returnObject;
    }

    private static string BuildRunShortcutUrl(string shortcutName, string apiBaseUrl, string deviceKey)
    {
        var payload = $"{{\"apiBaseUrl\":\"{apiBaseUrl}\",\"deviceKey\":\"{deviceKey}\"}}";
        var encodedName = Uri.EscapeDataString(shortcutName);
        var encodedText = Uri.EscapeDataString(payload);

        // Shortcuts URL scheme:
        // shortcuts://run-shortcut?name=NAME&input=text&text=PAYLOAD
        return $"shortcuts://run-shortcut?name={encodedName}&input=text&text={encodedText}";
    }

    private static Bitmap CreateQR(string payload, int pixelsPerModule = 12)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qr = new QRCode(data);

        return qr.GetGraphic(pixelsPerModule);
    }
}

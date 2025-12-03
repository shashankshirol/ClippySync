using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ClippySync.Web;

public abstract class Util
{
    public static string? GetLocalIPv4()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;
            var skiptypes = new[]
            {
                NetworkInterfaceType.Unknown,
                NetworkInterfaceType.Loopback,
                NetworkInterfaceType.Tunnel,
                NetworkInterfaceType.Ppp
            };
            if (skiptypes.Contains(ni.NetworkInterfaceType))
                continue;

            foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    return ip.Address.ToString();
        }

        return null;
    }
}
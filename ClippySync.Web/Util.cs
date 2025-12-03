using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ClippySync.Web;

public abstract class Util
{
    public static string? GetLocalIPv4()
    {
        var networkInts = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var ni in networkInts)
        { 
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;
            var allowedTypes = new[]
            {
                NetworkInterfaceType.Ethernet, 
                NetworkInterfaceType.Wireless80211,  
            }; 
            if (!allowedTypes.Contains(ni.NetworkInterfaceType))
                continue;

            // skip virtual adapters
            string[] virtualKeywords =
            {
                "virtual", "vmware", "hyper-v", "vbox", "virtualbox",
                "wsl", "vpn", "wireguard", "wg", "nord", "tap", "tun"
            };

            if (virtualKeywords.Any(v =>
                ni.Name.Contains(v, StringComparison.OrdinalIgnoreCase) ||
                ni.Description.Contains(v, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var addresses = ni.GetIPProperties().UnicastAddresses;
            foreach (var ip in addresses) 
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork) 
                    return ip.Address.ToString(); 
        } 

        return null; 
    }
}
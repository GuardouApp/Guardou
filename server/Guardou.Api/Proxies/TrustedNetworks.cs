using System.Net;

namespace Guardou.Api.Proxies;

internal static class TrustedNetworks
{
    public static IReadOnlyList<IPNetwork>? TryParse(string? value)
    {
        List<IPNetwork> networks = [];
        foreach (string part in (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (TryParseNetwork(part) is not { } network)
            {
                return null;
            }

            networks.Add(network);
        }

        return networks;
    }

    private static IPNetwork? TryParseNetwork(string part)
    {
        if (IPNetwork.TryParse(part, out IPNetwork network))
        {
            return network;
        }

        return IPAddress.TryParse(part, out IPAddress? address)
            ? new IPNetwork(address, address.GetAddressBytes().Length * 8)
            : null;
    }
}

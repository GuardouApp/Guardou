using NetVips;

namespace Guardou.Infrastructure.Previews;

internal static class VipsDefaults
{
    public static void Apply()
    {
        NetVips.NetVips.BlockUntrusted = true;
        Cache.Max = 0;
    }
}

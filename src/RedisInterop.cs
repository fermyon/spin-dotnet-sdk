using System.Runtime.CompilerServices;

namespace Fermyon.Spin.Sdk;

internal static class OutboundRedisInterop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte outbound_redis_get(ref InteropString address, ref InteropString key, ref Buffer ret0);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte outbound_redis_set(ref InteropString address, ref InteropString key, ref Buffer value);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte outbound_redis_publish(ref InteropString address, ref InteropString channel, ref Buffer value);
}

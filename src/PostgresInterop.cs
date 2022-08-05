using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Fermyon.Spin.Sdk;

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct PgPayloadList {
    private readonly Buffer* _ptr;
    private readonly int _len;

    internal List<Buffer> AsList()
    {
        var list = new List<Buffer>(_len);
        for (int i = 0; i < _len; ++i)
        {
            Buffer* ptr = _ptr + i;
            list.Add(*ptr);
        }
        return list;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct PgPayloadListList {
    private readonly PgPayloadList* _ptr;
    private readonly int _len;

    internal List<List<Buffer>> AsList()
    {
        var list = new List<List<Buffer>>(_len);
        for (int i = 0; i < _len; ++i)
        {
            PgPayloadList* ptr = _ptr + i;
            list.Add(ptr->AsList());
        }
        return list;
    }
}

internal static class OutboundPgInterop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte outbound_pg_query(ref InteropString address, ref InteropString statement, ref InteropStringList parameters, ref PgPayloadListList ret0);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe byte outbound_pg_execute(ref InteropString address, ref InteropString statement, ref InteropStringList parameters, ref UInt64 ret0);
}

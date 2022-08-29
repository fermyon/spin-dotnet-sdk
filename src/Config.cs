using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fermyon.Spin.Sdk;

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct ConfigError {
    internal const byte SPIN_CONFIG_ERROR_PROVIDER = 0;
    internal const byte SPIN_CONFIG_ERROR_INVALID_KEY = 1;
    internal const byte SPIN_CONFIG_ERROR_INVALID_SCHEMA = 2;
    internal const byte SPIN_CONFIG_ERROR_OTHER = 3;

    internal readonly byte tag;
    internal readonly InteropString message;
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe readonly struct ConfigResultValue {
    [FieldOffset(0)]
    internal readonly InteropString ok;
    [FieldOffset(0)]
    internal readonly ConfigError err;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct ConfigResult {
    internal readonly byte is_err;
    internal readonly ConfigResultValue val;
}

internal static class SpinConfigNative
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe void spin_config_get_config(ref InteropString key, out ConfigResult ret0);
}

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Fermyon.Spin.Sdk;

internal class Interop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern unsafe void Test(float a);
}

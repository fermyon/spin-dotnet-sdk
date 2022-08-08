using System.Runtime.InteropServices;
using System.Text;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Buffer : IEnumerable<byte>
{
    private readonly nint _ptr;
    private readonly int _length;

    private Buffer(nint ptr, int length)
    {
        _ptr = ptr;
        _length = length;
    }

    public unsafe ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>((void*)_ptr, _length);

    public static unsafe Buffer FromString(string value)
    {
        var interopString = InteropString.FromString(value);
        return new Buffer(interopString._utf8Ptr, interopString._utf8Length);
    }

    public static unsafe Buffer FromBytes(IEnumerable<byte> value)
    {
        // We materialise this so as to get its length and avoid traversing twice,
        // but it does seem wasteful.  TODO: better way?
        var source = new Span<byte>(value.ToArray());
        var exactByteCount = source.Length;
        var mem = Marshal.AllocHGlobal(exactByteCount);
        var buffer = new Span<byte>((void*)mem, exactByteCount);
        source.CopyTo(buffer);
        return new Buffer(mem, exactByteCount);
    }

    public string ToUTF8String()
    {
        return Encoding.UTF8.GetString(this.AsSpan());
    }

    internal InteropString ToInteropString()
        => new InteropString(_ptr, _length);

    public IEnumerator<byte> GetEnumerator() => new Enumerator(_ptr, _length);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    
    private unsafe struct Enumerator : IEnumerator<byte>
    {
        private readonly byte* _ptr;
        private readonly int _length;
        private int _index = -1;

        public Enumerator(nint ptr, int length)
        {
            _ptr = (byte*)ptr;
            _length = length;
        }

        public byte Current
        {
            get
            {
                if (_index < 0 || _index >= _length)
                {
                    throw new InvalidOperationException();
                }
                var ptr = _ptr + _index;
                return *ptr;
            }
        }

        public bool MoveNext()
        {
            ++_index;
            return _index < _length;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object System.Collections.IEnumerator.Current => Current;
        void IDisposable.Dispose() {}
    }

}

public static class OptionalBufferExtensions
{
    public static bool HasContent(this Optional<Buffer> buffer)
    {
        return buffer.TryGetValue(out var _);
    }

    public static ReadOnlySpan<byte> AsBytes(this Optional<Buffer> buffer)
    {
        if (buffer.TryGetValue(out var value))
        {
            return value.AsSpan();
        }
        return new ReadOnlySpan<byte>(Array.Empty<byte>());
    }

    public static string AsString(this Optional<Buffer> buffer)
    {
        if (buffer.TryGetValue(out var value))
        {
            return value.ToUTF8String();
        }
        return String.Empty;
    }
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct Optional<T>
    where T: struct
{
    private readonly byte _isSome;
    private readonly T _value;

    internal Optional(T value)
    {
        _isSome = 1;
        _value = value;
    }

    public bool TryGetValue(out T value)
    {
        value = _value;
        return _isSome != 0;
    }

    public static readonly Optional<T> None = default;

    public static implicit operator T?(Optional<T> opt) =>
        opt._isSome == 0 ? null : opt._value;
}

public static class Optional
{
    // Just so the caller doesn't have to specify <T>
    public static Optional<T> From<T>(T value) where T: struct => new Optional<T>(value);
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct InteropString
{
    internal readonly nint _utf8Ptr;
    internal readonly int _utf8Length;

    internal InteropString(nint ptr, int length)
    {
        _utf8Ptr = ptr;
        _utf8Length = length;
    }

    public override string ToString()
        => Marshal.PtrToStringUTF8(_utf8Ptr, _utf8Length);

    public static unsafe InteropString FromString(string value)
    {
        var exactByteCount = checked(Encoding.UTF8.GetByteCount(value));
        var mem = Marshal.AllocHGlobal(exactByteCount);
        var buffer = new Span<byte>((void*)mem, exactByteCount);
        int byteCount = Encoding.UTF8.GetBytes(value, buffer);
        return new InteropString(mem, byteCount);
    }
}

using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// An umanaged contiguous span of bytes.
/// </summary>
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

    /// <summary>
    /// Gets the length of the Buffer.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets the contents of the Buffer as a Span.
    /// </summary>
    public unsafe ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>((void*)_ptr, _length);

    /// <summary>
    /// Creates a Buffer containing a string, encoding it using UTF-8.
    /// </summary>
    public static unsafe Buffer FromString(string value)
    {
        var interopString = InteropString.FromString(value);
        return new Buffer(interopString._utf8Ptr, interopString._utf8Length);
    }

    /// <summary>
    /// Creates a Buffer containing a sequence of bytes.
    /// </summary>
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

    /// <summary>
    /// Gets the contents of the Buffer as a string, interpreting it using
    /// UTF-8 encoding.
    /// </summary>
    public string ToUTF8String()
    {
        return Encoding.UTF8.GetString(this.AsSpan());
    }

    internal InteropString ToInteropString()
        => new InteropString(_ptr, _length);

    /// <summary>
    /// Gets an iterator over the bytes in the Buffer.
    /// </summary>
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

/// <summary>
/// Convenience methods for decoding optional Buffers.
/// </summary>
public static class OptionalBufferExtensions
{
    /// <summary>
    /// Gets whether the option contains any content.
    /// </summary>
    public static bool HasContent(this Optional<Buffer> buffer)
    {
        return buffer.TryGetValue(out var value) && (value.Length > 0);
    }

    /// <summary>
    /// Gets the contents of the contained Buffer as a Span.  If the Optional
    /// is None, the Span is empty.
    /// </summary>
    public static ReadOnlySpan<byte> AsBytes(this Optional<Buffer> buffer)
    {
        if (buffer.TryGetValue(out var value))
        {
            return value.AsSpan();
        }
        return new ReadOnlySpan<byte>(Array.Empty<byte>());
    }

    /// <summary>
    /// Gets the contents of the contained Buffer as a string.  If the Optional
    /// is None, the string is empty.
    /// </summary>
    public static string AsString(this Optional<Buffer> buffer)
    {
        if (buffer.TryGetValue(out var value))
        {
            return value.ToUTF8String();
        }
        return String.Empty;
    }
}

/// <summary>
/// An unmanaged struct that may or may not contain a T.
/// </summary>
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

    /// <summary>
    /// Gets the contents of the Optional, if any.  If the Optional is None, it returns
    /// false and the out variable is undefined.  If the Optional contains a value,
    /// returns true and the value is copied into the out variable.
    /// </summary>
    public bool TryGetValue(out T value)
    {
        value = _value;
        return _isSome != 0;
    }

    /// <summary>
    /// An Optional representing the absence of a value.
    /// </summary>
    public static readonly Optional<T> None = default;

    /// <summary>
    /// Surfaces the Optional as a C# nullable type.
    /// </summary>
    public static implicit operator T?(Optional<T> opt) =>
        opt._isSome == 0 ? null : opt._value;
}

/// <summary>
/// Convenience methods for constructing Optionals.
/// </summary>
public static class Optional
{
    /// <summary>
    /// Constructs an Optional containing the specified value.
    /// </summary>
    public static Optional<T> From<T>(T value) where T: struct => new Optional<T>(value);
}

/// <summary>
/// The Wasm Canonical ABI representation of a string.
/// </summary>
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

    /// <summary>
    /// Gets the string represented by the InteropString.
    /// </summary>
    public override string ToString()
        => Marshal.PtrToStringUTF8(_utf8Ptr, _utf8Length);

    /// <summary>
    /// Creates the Canonical ABI representation from a .NET string.
    /// </summary>
    public static unsafe InteropString FromString(string value)
    {
        var exactByteCount = checked(Encoding.UTF8.GetByteCount(value));
        var mem = Marshal.AllocHGlobal(exactByteCount);
        var buffer = new Span<byte>((void*)mem, exactByteCount);
        int byteCount = Encoding.UTF8.GetBytes(value, buffer);
        return new InteropString(mem, byteCount);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct InteropStringList
{
    private readonly InteropString* _ptr;
    private readonly int _len;

    internal InteropStringList(InteropString* ptr, int length)
    {
        _ptr = ptr;
        _len = length;
    }

    internal static InteropStringList FromStrings(string[] values)
    {
        var unmanagedValues = (InteropString*)Marshal.AllocHGlobal(values.Length * sizeof(InteropString));
        var span = new Span<InteropString>(unmanagedValues, values.Length);
        var index = 0;
        foreach (var value in values)
        {
            span[index] = InteropString.FromString(value);
            index++;
        }
        return new InteropStringList(unmanagedValues, values.Length);
    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct InteropList<T> : IEnumerable<T>
    where T: unmanaged
{
    internal readonly T* _ptr;
    internal readonly int _len;

    private InteropList(T* ptr, int len)
    {
        _ptr = ptr;
        _len = len;
    }

    public int Count => _len;

    public static InteropList<T> From(T[] values)
    {
        var sourceSpan = new Span<T>(values);

        var unmanagedValues = (T*)Marshal.AllocHGlobal(values.Length * sizeof(T));
        var span = new Span<T>(unmanagedValues, values.Length);
        sourceSpan.CopyTo(span);
        return new InteropList<T>(unmanagedValues, values.Length);
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator(_ptr, _len);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private struct Enumerator : IEnumerator<T>
    {
        private T* _ptr;
        private int _len;
        private int _index = -1;

        public Enumerator(T* ptr, int len)
        {
            _ptr = ptr;
            _len = len;
        }

        public T Current
        {
            get
            {
                if (_index < 0 || _index >= _len)
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
            return _index < _len;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object System.Collections.IEnumerator.Current => Current;
        void IDisposable.Dispose() {}
    }
}

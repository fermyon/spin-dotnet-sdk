using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Fermyon.Spin.Sdk;

[StructLayout(LayoutKind.Explicit)]
public unsafe readonly struct DbValue {
    internal const byte OUTBOUND_PG_DB_VALUE_BOOLEAN = 0;
    internal const byte OUTBOUND_PG_DB_VALUE_INT8 = 1;
    internal const byte OUTBOUND_PG_DB_VALUE_INT16 = 2;
    internal const byte OUTBOUND_PG_DB_VALUE_INT32 = 3;
    internal const byte OUTBOUND_PG_DB_VALUE_INT64 = 4;
    internal const byte OUTBOUND_PG_DB_VALUE_UINT8 = 5;
    internal const byte OUTBOUND_PG_DB_VALUE_UINT16 = 6;
    internal const byte OUTBOUND_PG_DB_VALUE_UINT32 = 7;
    internal const byte OUTBOUND_PG_DB_VALUE_UINT64 = 8;
    internal const byte OUTBOUND_PG_DB_VALUE_FLOATING32 = 9;
    internal const byte OUTBOUND_PG_DB_VALUE_FLOATING64 = 10;
    internal const byte OUTBOUND_PG_DB_VALUE_STR = 11;
    internal const byte OUTBOUND_PG_DB_VALUE_BINARY = 12;
    internal const byte OUTBOUND_PG_DB_VALUE_DB_NULL = 13;
    internal const byte OUTBOUND_PG_DB_VALUE_UNSUPPORTED = 14;

    [FieldOffset(0)]
    internal readonly byte tag;
    [FieldOffset(8)]
    internal readonly bool boolean;
    [FieldOffset(8)]
    internal readonly sbyte int8;
    [FieldOffset(8)]
    internal readonly Int16 int16;
    [FieldOffset(8)]
    internal readonly Int32 int32;
    [FieldOffset(8)]
    internal readonly Int64 int64;
    [FieldOffset(8)]
    internal readonly byte uint8;
    [FieldOffset(8)]
    internal readonly UInt16 uint16;
    [FieldOffset(8)]
    internal readonly UInt32 uint32;
    [FieldOffset(8)]
    internal readonly UInt64 uint64;
    [FieldOffset(8)]
    internal readonly float floating32;
    [FieldOffset(8)]
    internal readonly double floating64;
    [FieldOffset(8)]
    internal readonly InteropString str;
    [FieldOffset(8)]
    internal readonly Buffer binary;

    public object? Value()
    {
        switch (tag)
        {
            case OUTBOUND_PG_DB_VALUE_BOOLEAN: return boolean;
            case OUTBOUND_PG_DB_VALUE_INT8: return int8;
            case OUTBOUND_PG_DB_VALUE_INT16: return int16;
            case OUTBOUND_PG_DB_VALUE_INT32: return int32;
            case OUTBOUND_PG_DB_VALUE_INT64: return int64;
            case OUTBOUND_PG_DB_VALUE_UINT8: return uint8;
            case OUTBOUND_PG_DB_VALUE_UINT16: return uint16;
            case OUTBOUND_PG_DB_VALUE_UINT32: return uint32;
            case OUTBOUND_PG_DB_VALUE_UINT64: return uint64;
            case OUTBOUND_PG_DB_VALUE_FLOATING32: return floating32;
            case OUTBOUND_PG_DB_VALUE_FLOATING64: return floating64;
            case OUTBOUND_PG_DB_VALUE_STR: return str.ToString();
            case OUTBOUND_PG_DB_VALUE_BINARY: return binary.ToString();
            case OUTBOUND_PG_DB_VALUE_DB_NULL: return null;
            default: throw new InvalidOperationException($"Spin doesn't support type {tag}");
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public unsafe readonly struct ParameterValue {
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_BOOLEAN = 0;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT8 = 1;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT16 = 2;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT32 = 3;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT64 = 4;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_UINT8 = 5;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_UINT16 = 6;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_UINT32 = 7;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_UINT64 = 8;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_FLOATING32 = 9;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_FLOATING64 = 10;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_STR = 11;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_BINARY = 12;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_DB_NULL = 13;

    public ParameterValue(bool value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_BOOLEAN;
        boolean = value;
    }

    public ParameterValue(sbyte value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_INT8;
        int8 = value;
    }

    public ParameterValue(short value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_INT16;
        int16 = value;
    }

    public ParameterValue(int value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_INT32;
        int32 = value;
    }

    public ParameterValue(long value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_INT64;
        int64 = value;
    }

    public ParameterValue(float value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_FLOATING32;
        floating32 = value;
    }

    public ParameterValue(double value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_FLOATING64;
        floating64 = value;
    }

    public ParameterValue(string value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_STR;
        str = InteropString.FromString(value);
    }

    public ParameterValue(IEnumerable<byte> value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_BINARY;
        binary = Buffer.FromBytes(value);
    }

    public ParameterValue(object? value) : this()
    {
        if (value is null)
        {
            tag = OUTBOUND_PG_PARAMETER_VALUE_DB_NULL;
        }
        else
        {
            throw new ArgumentException(nameof(value));
        }
    }

    public static ParameterValue From(object? value)
    {
        return value switch
        {
            bool v => new ParameterValue(v),
            byte v => new ParameterValue(v),
            short v => new ParameterValue(v),
            int v => new ParameterValue(v),
            long v => new ParameterValue(v),
            float v => new ParameterValue(v),
            double v => new ParameterValue(v),
            string v => new ParameterValue(v),
            IEnumerable<byte> v => new ParameterValue(v),
            null => new ParameterValue((object?)null),
            _ => throw new ArgumentException(nameof(value))
        };
    }

    [FieldOffset(0)]
    internal readonly byte tag;
    [FieldOffset(8)]
    internal readonly bool boolean;
    [FieldOffset(8)]
    internal readonly sbyte int8;
    [FieldOffset(8)]
    internal readonly Int16 int16;
    [FieldOffset(8)]
    internal readonly Int32 int32;
    [FieldOffset(8)]
    internal readonly Int64 int64;
    [FieldOffset(8)]
    internal readonly byte uint8;
    [FieldOffset(8)]
    internal readonly UInt16 uint16;
    [FieldOffset(8)]
    internal readonly UInt32 uint32;
    [FieldOffset(8)]
    internal readonly UInt64 uint64;
    [FieldOffset(8)]
    internal readonly float floating32;
    [FieldOffset(8)]
    internal readonly double floating64;
    [FieldOffset(8)]
    internal readonly InteropString str;
    [FieldOffset(8)]
    internal readonly Buffer binary;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRow : IReadOnlyList<DbValue> {
    internal readonly DbValue* _ptr;
    internal readonly int _len;

    public int Count => _len;

    public DbValue this[int index]
    {
        get
        {
            if (index < 0 || index >= _len)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var ptr = _ptr + index;
            return *ptr;
        }
    }

    public IEnumerator<DbValue> GetEnumerator() => new Enumerator(_ptr, _len);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    // TODO: surely we can make this generic by now
    private struct Enumerator : IEnumerator<DbValue>
    {
        private DbValue* _ptr;
        private int _len;
        private int _index = -1;

        public Enumerator(DbValue* ptr, int len)
        {
            _ptr = ptr;
            _len = len;
        }

        public DbValue Current
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

[StructLayout(LayoutKind.Sequential)]
internal unsafe readonly struct PgError {
    internal const byte OUTBOUND_PG_PG_ERROR_SUCCESS = 0;
    internal const byte OUTBOUND_PG_PG_ERROR_CONNECTION_FAILED = 1;
    internal const byte OUTBOUND_PG_PG_ERROR_BAD_PARAMETER = 2;
    internal const byte OUTBOUND_PG_PG_ERROR_QUERY_FAILED = 3;
    internal const byte OUTBOUND_PG_PG_ERROR_VALUE_CONVERSION_FAILED = 4;
    internal const byte OUTBOUND_PG_PG_ERROR_OTHER_ERROR = 5;

    // NOTE: this relies on all variants with data having the same layout!
    internal readonly byte tag;
    internal readonly InteropString message;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRows : IEnumerable<PgRow> {
    internal readonly PgRow* _ptr;
    internal readonly int _len;

    public int Count => _len;

    public IEnumerator<PgRow> GetEnumerator() => new Enumerator(_ptr, _len);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private struct Enumerator : IEnumerator<PgRow>
    {
        private PgRow* _ptr;
        private int _len;
        private int _index = -1;

        public Enumerator(PgRow* ptr, int len)
        {
            _ptr = ptr;
            _len = len;
        }

        public PgRow Current
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

public enum PgDataType : byte
{
    OUTBOUND_PG_DB_DATA_TYPE_BOOLEAN = 0,
    OUTBOUND_PG_DB_DATA_TYPE_INT8 = 1,
    OUTBOUND_PG_DB_DATA_TYPE_INT16 = 2,
    OUTBOUND_PG_DB_DATA_TYPE_INT32 = 3,
    OUTBOUND_PG_DB_DATA_TYPE_INT64 = 4,
    OUTBOUND_PG_DB_DATA_TYPE_UINT8 = 5,
    OUTBOUND_PG_DB_DATA_TYPE_UINT16 = 6,
    OUTBOUND_PG_DB_DATA_TYPE_UINT32 = 7,
    OUTBOUND_PG_DB_DATA_TYPE_UINT64 = 8,
    OUTBOUND_PG_DB_DATA_TYPE_FLOATING32 = 9,
    OUTBOUND_PG_DB_DATA_TYPE_FLOATING64 = 10,
    OUTBOUND_PG_DB_DATA_TYPE_STR = 11,
    OUTBOUND_PG_DB_DATA_TYPE_BINARY = 12,
    OUTBOUND_PG_DB_DATA_TYPE_OTHER = 13,
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgColumn
{
    internal readonly InteropString name;
    internal readonly PgDataType data_type;

    public string Name => name.ToString();
    public PgDataType DataType => data_type;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRowSet {
    internal readonly InteropList<PgColumn> _columns;
    internal readonly PgRows _rows;

    public IEnumerable<PgColumn> Columns => _columns;
    public PgRows Rows => _rows;
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe readonly struct PgU64OrError {
    [FieldOffset(0)]
    internal readonly byte is_err;
    [FieldOffset(8)]
    internal readonly UInt64 value;
    [FieldOffset(8)]
    internal readonly PgError err;
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe readonly struct PgRowSetOrError {
    [FieldOffset(0)]
    internal readonly byte is_err;
    [FieldOffset(4)]
    internal readonly PgRowSet value;
    [FieldOffset(4)]
    internal readonly PgError err;
}

internal static class OutboundPgInterop
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe void outbound_pg_query(ref InteropString address, ref InteropString statement, ref InteropList<ParameterValue> parameters, ref PgRowSetOrError ret0);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern unsafe void outbound_pg_execute(ref InteropString address, ref InteropString statement, ref InteropList<ParameterValue> parameters, ref PgU64OrError ret0);
}

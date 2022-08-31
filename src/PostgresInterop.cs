using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Fermyon.Spin.Sdk;

/// <summary>
/// A value retrieved from a Postgres database.
/// </summary>
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

    /// <summary>
    /// Gets the value as a .NET object.
    /// </summary>
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
            case OUTBOUND_PG_DB_VALUE_BINARY: return binary;
            case OUTBOUND_PG_DB_VALUE_DB_NULL: return null;
            default: throw new InvalidOperationException($"Spin doesn't support type {tag}");
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe readonly struct ParameterValue {
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
            _ => throw new ArgumentException($"No conversion for type '{value.GetType().FullName}'", nameof(value))
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

/// <summary>
/// A row retrieved from a Postgres database.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRow : IReadOnlyList<DbValue> {
    internal readonly DbValue* _ptr;
    internal readonly int _len;

    /// <summary>
    /// The number of columns in the row.
    /// </summary>
    public int Count => _len;

    /// <summary>
    /// Gets the value of the column at the specified index.
    /// </summary>
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

    /// <summary>
    /// Gets an iterator over the values in the row.
    /// </summary>
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

/// <summary>
/// A list of rows retrieved from a Postgres database.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRows : IReadOnlyList<PgRow> {
    internal readonly PgRow* _ptr;
    internal readonly int _len;

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    public int Count => _len;

    /// <summary>
    /// Gets the row at the specified index.
    /// </summary>
    public PgRow this[int index]
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

    /// <summary>
    /// Gets an iterator over the rows.
    /// </summary>
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

/// <summary>
/// The data type of a Postgres column.
/// </summary>
public enum PgDataType : byte
{
    /// <summary>
    /// Boolean data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_BOOLEAN = 0,
    /// <summary>
    /// 8-bit signed integer (sbyte) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_INT8 = 1,
    /// <summary>
    /// 16-bit signed integer (short) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_INT16 = 2,
    /// <summary>
    /// 32-bit signed integer (int) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_INT32 = 3,
    /// <summary>
    /// 64-bit signed integer (long) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_INT64 = 4,
    /// <summary>
    /// 8-bit unsigned integer (byte) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_UINT8 = 5,
    /// <summary>
    /// 16-bit unsigned integer (ushort) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_UINT16 = 6,
    /// <summary>
    /// 32-bit unsigned integer (uint) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_UINT32 = 7,
    /// <summary>
    /// 64-bit unsigned integer (ulong) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_UINT64 = 8,
    /// <summary>
    /// 32-bit floating point (float) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_FLOATING32 = 9,
    /// <summary>
    /// 64-bit floating point (double) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_FLOATING64 = 10,
    /// <summary>
    /// String data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_STR = 11,
    /// <summary>
    /// Binary blob (Buffer) data type.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_BINARY = 12,
    /// <summary>
    /// Any data type not supported by Spin.
    /// </summary>
    OUTBOUND_PG_DB_DATA_TYPE_OTHER = 13,
}

/// <summary>
/// Column metadata from a Postgres database.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgColumn
{
    internal readonly InteropString name;
    internal readonly PgDataType data_type;

    /// <summary>
    /// Gets the name of the column.
    /// </summary>
    public string Name => name.ToString();
    /// <summary>
    /// Gets the data type of the column.
    /// </summary>
    public PgDataType DataType => data_type;
}

/// <summary>
/// The result of a query to a Postgres database.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRowSet {
    internal readonly InteropList<PgColumn> _columns;
    internal readonly PgRows _rows;

    /// <summary>
    /// Gets the columns retrieved by the query.
    /// </summary>
    public IEnumerable<PgColumn> Columns => _columns;
    /// <summary>
    /// Gets the rows retrieved by the query.
    /// </summary>
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

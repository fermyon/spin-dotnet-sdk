using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Fermyon.Spin.Sdk;

[StructLayout(LayoutKind.Explicit)]
public unsafe readonly struct DbValue {
    internal const byte OUTBOUND_PG_DB_VALUE_BOOLEAN = 0;
    internal const byte OUTBOUND_PG_DB_VALUE_INT32 = 1;
    internal const byte OUTBOUND_PG_DB_VALUE_INT64 = 2;
    internal const byte OUTBOUND_PG_DB_VALUE_DB_STRING = 3;
    internal const byte OUTBOUND_PG_DB_VALUE_DB_NULL = 4;
    internal const byte OUTBOUND_PG_DB_VALUE_UNSUPPORTED = 5;

    [FieldOffset(0)]
    internal readonly byte tag;
    [FieldOffset(8)]
    internal readonly bool boolean;
    [FieldOffset(8)]
    internal readonly Int32 int32;
    [FieldOffset(8)]
    internal readonly Int64 int64;
    [FieldOffset(8)]
    internal readonly InteropString db_string;

    public object? Value()
    {
        switch (tag)
        {
            case OUTBOUND_PG_DB_VALUE_BOOLEAN: return boolean;
            case OUTBOUND_PG_DB_VALUE_INT32: return int32;
            case OUTBOUND_PG_DB_VALUE_INT64: return int64;
            case OUTBOUND_PG_DB_VALUE_DB_STRING: return db_string.ToString();
            case OUTBOUND_PG_DB_VALUE_DB_NULL: return null;
            default: return $"<NO CAN DO {tag}>"; //throw new InvalidOperationException($"Spin doesn't support type {tag}");
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public unsafe readonly struct ParameterValue {
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_BOOLEAN = 0;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT32 = 1;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_INT64 = 2;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_DB_STRING = 3;
    internal const byte OUTBOUND_PG_PARAMETER_VALUE_DB_NULL = 4;

    public ParameterValue(bool value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_BOOLEAN;
        boolean = value;
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

    public ParameterValue(string value) : this()
    {
        tag = OUTBOUND_PG_PARAMETER_VALUE_DB_STRING;
        db_string = InteropString.FromString(value);
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
            int v => new ParameterValue(v),
            long v => new ParameterValue(v),
            string v => new ParameterValue(v),
            null => new ParameterValue((object?)null),
            _ => throw new ArgumentException(nameof(value))
        };
    }

    [FieldOffset(0)]
    internal readonly byte tag;
    [FieldOffset(8)]
    internal readonly bool boolean;
    [FieldOffset(8)]
    internal readonly Int32 int32;
    [FieldOffset(8)]
    internal readonly Int64 int64;
    [FieldOffset(8)]
    internal readonly InteropString db_string;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct PgRow : IEnumerable<DbValue> {
    internal readonly DbValue* _ptr;
    internal readonly int _len;

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
    internal const byte OUTBOUND_PG_PG_ERROR_QUERY_FAILED = 2;
    internal const byte OUTBOUND_PG_PG_ERROR_OTHER_ERROR = 3;

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
    OUTBOUND_PG_DB_DATA_TYPE_INT32 = 1,
    OUTBOUND_PG_DB_DATA_TYPE_INT64 = 2,
    OUTBOUND_PG_DB_DATA_TYPE_DB_STRING = 3,
    OUTBOUND_PG_DB_DATA_TYPE_OTHER = 4,
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
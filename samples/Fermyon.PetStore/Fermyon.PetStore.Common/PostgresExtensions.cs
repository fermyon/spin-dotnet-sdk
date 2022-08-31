using Fermyon.Spin.Sdk;

namespace Fermyon.PetStore.Common;

public static class PostgresExtensions
{
    public static int AsInt32(this DbValue value)
    {
        return value.Value() switch
        {
            null => throw new InvalidOperationException("value is null"),
            var v => (int)v,
        };
    }

    public static int? AsNullableInt32(this DbValue value)
    {
        return value.Value() switch
        {
            null => null,
            var v => (int)v,
        };
    }

    public static string AsString(this DbValue value)
    {
        return value.Value() switch
        {
            null => throw new InvalidOperationException("value is null"),
            var v => (string)v,
        };
    }

    public static Buffer AsBuffer(this DbValue value)
    {
        return value.Value() switch
        {
            null => throw new InvalidOperationException("value is null"),
            var v => (Buffer)v,
        };
    }

    public static Buffer? AsNullableBuffer(this DbValue value)
    {
        return value.Value() switch
        {
            null => null,
            var v => (Buffer)v,
        };
    }
}

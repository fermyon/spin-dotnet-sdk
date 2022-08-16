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

    public static string AsString(this DbValue value)
    {
        return value.Value() switch
        {
            null => throw new InvalidOperationException("value is null"),
            var v => (string)v,
        };
    }
}

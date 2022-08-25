namespace Fermyon.Spin.Sdk;

/// <summary>
/// Performs operations on a Postgres database.
/// </summary>
public static class PostgresOutbound
{
    /// <summary>
    /// Performs a query against a Postgres database.
    /// </summary>
    // TODO: less foul return type like an IDataReader or something
    public static PgRowSet Query(string connectionString, string sql, params object?[] parameters)
    {
        var conn = InteropString.FromString(connectionString);
        var stmt = InteropString.FromString(sql);
        var parms = InteropList<ParameterValue>.From(parameters.Select(p => ParameterValue.From(p)).ToArray());
        var result = new PgRowSetOrError();

        OutboundPgInterop.outbound_pg_query(ref conn, ref stmt, ref parms, ref result);

        if (result.is_err == 0)
        {
            return result.value;
        }
        else
        {
            var err = result.err;
            throw new Exception($"Postgres query error: interop error {err.tag}: {err.message.ToString()}");
        }
    }

    /// <summary>
    /// Executes a SQL statement against a Postgres database, and returns the number of rows changed. This
    /// is for statements that do not return a result set.
    /// </summary>
    public static long Execute(string connectionString, string sql, params object?[] parameters)
    {
        var conn = InteropString.FromString(connectionString);
        var stmt = InteropString.FromString(sql);
        var parms = InteropList<ParameterValue>.From(parameters.Select(p => ParameterValue.From(p)).ToArray());
        var result = new PgU64OrError();

        OutboundPgInterop.outbound_pg_execute(ref conn, ref stmt, ref parms, ref result);

        if (result.is_err == 0)
        {
            return (long)(result.value);
        }
        else
        {
            var err = result.err;
            throw new Exception($"Postgres execute error: interop error {err.tag}: {err.message.ToString()}");
        }
    }
}

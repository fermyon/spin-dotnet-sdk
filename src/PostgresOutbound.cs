namespace Fermyon.Spin.Sdk;

public static class PostgresOutbound
{
    // TODO: less foul return type like an IDataReader or something
    public static PgRowSet Query(string connectionString, string sql, params string[] parameters)
    {
        var conn = InteropString.FromString(connectionString);
        var stmt = InteropString.FromString(sql);
        var parms = InteropStringList.FromStrings(parameters);
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
}

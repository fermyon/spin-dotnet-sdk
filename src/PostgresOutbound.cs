namespace Fermyon.Spin.Sdk;

public static class PostgresOutbound
{
    // TODO: less foul return type like an IDataReader or something
    public static List<List<Buffer>> Query(string connectionString, string sql, params string[] parameters)
    {
        var conn = InteropString.FromString(connectionString);
        var stmt = InteropString.FromString(sql);
        var parms = InteropStringList.FromStrings(parameters);
        var result = new PgPayloadListList();

        var error = OutboundPgInterop.outbound_pg_query(ref conn, ref stmt, ref parms, ref result);

        if (error == 0 || error == 255)
        {
            return result.AsList();
        }
        else
        {
            throw new Exception($"Postgres query error: interop error {error}");
        }
    }
}

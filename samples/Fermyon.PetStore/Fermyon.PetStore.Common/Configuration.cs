using Fermyon.Spin.Sdk;

namespace Fermyon.PetStore.Common;

public static class Configuration
{
    public static string DbConnectionString()
    {
        return SpinConfig.Get("pg_conn_str");
    }
}
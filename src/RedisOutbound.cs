namespace Fermyon.Spin.Sdk;

public static class RedisOutbound
{
    public static Buffer Get(string address, string key)
    {
        var res = new Buffer();
        var redisAddress = InteropString.FromString(address);
        var redisKey = InteropString.FromString(key);

        var err = OutboundRedisInterop.outbound_redis_get(ref redisAddress, ref redisKey, ref res);
        if (err == 0 || err == 255)
        {
            return res;
        }
        else
        {
            throw new Exception("Redis outbound error: cannot GET.");
        }
    }

    public static void Set(string address, string key, Buffer payload)
    {
        var redisAddress = InteropString.FromString(address);
        var redisKey = InteropString.FromString(key);

        var err = OutboundRedisInterop.outbound_redis_set(ref redisAddress, ref redisKey, ref payload);
        if (err == 0 || err == 255)
        {
            return;
        }
        else
        {
            throw new Exception("Redis outbound error: cannot SET.");
        }
    }

    public static void Publish(string address, string channel, Buffer payload)
    {
        var redisAddress = InteropString.FromString(address);
        var redisChannel = InteropString.FromString(channel);

        var err = OutboundRedisInterop.outbound_redis_publish(ref redisAddress, ref redisChannel, ref payload);
        if (err == 0 || err == 255)
        {
            return;
        }
        else
        {
            throw new Exception("Redis outbound error: cannot PUBLISH.");
        }
    }
}

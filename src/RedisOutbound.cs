namespace Fermyon.Spin.Sdk;

/// <summary>
/// Performs operations on a Redis store.
/// </summary>
public static class RedisOutbound
{
    /// <summary>
    /// Gets the value of a key.
    /// </summary>
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

    /// <summary>
    /// Sets the value of a key. If the key already holds a value, it is overwritten.
    /// </summary>
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

    /// <summary>
    /// Publishes a Redis message to the specified channel.
    /// </summary>
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

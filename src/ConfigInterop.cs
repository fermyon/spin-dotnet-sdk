namespace Fermyon.Spin.Sdk;

/// <summary>
/// Gets component configuration values from Spin.
/// </summary>
public static class SpinConfig
{
    /// <summary>
    /// Gets the component configuration value identified by the key.
    /// </summary>
    public static string Get(string key)
    {
        var interopKey = InteropString.FromString(key);
        SpinConfigNative.spin_config_get_config(ref interopKey, out var result);

        if (result.is_err == 0)
        {
            return result.val.ok.ToString();
        }
        else
        {
            var message = result.val.err.message.ToString();
            Exception ex = result.val.err.tag switch {
                ConfigError.SPIN_CONFIG_ERROR_PROVIDER => new InvalidOperationException(message),
                ConfigError.SPIN_CONFIG_ERROR_INVALID_KEY => new ArgumentException($"Incorrect key syntax: {message}", nameof(key)),
                ConfigError.SPIN_CONFIG_ERROR_INVALID_SCHEMA => new InvalidOperationException($"Incorrect app configuration: {message}"),
                ConfigError.SPIN_CONFIG_ERROR_OTHER => new InvalidOperationException(message),
                _ => new Exception("Unknown error from Spin configuration service"),
            };
            throw ex;
        }
    }
}

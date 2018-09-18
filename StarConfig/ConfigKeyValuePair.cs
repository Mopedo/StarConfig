using Dynamit;

namespace StarConfig
{
    /// <inheritdoc />
    /// <summary>
    /// The key value pair class for use in config
    /// </summary>
    public class ConfigKeyValuePair : DKeyValuePair
    {
        /// <inheritdoc />
        public ConfigKeyValuePair(DDictionary dict, string key, object value = null) : base(dict, key, value) { }
    }
}
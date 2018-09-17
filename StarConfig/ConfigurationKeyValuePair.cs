using Dynamit;

namespace StarConfig
{
    public class ConfigurationKeyValuePair : DKeyValuePair
    {
        public ConfigurationKeyValuePair(DDictionary dict, string key, object value = null) : base(dict, key, value) { }
    }
}
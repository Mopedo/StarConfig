using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Starcounter;
using static System.Globalization.DateTimeStyles;
using Formatting = Newtonsoft.Json.Formatting;

namespace StarConfig
{
    /// <inheritdoc cref="DDictionary" />
    /// <inheritdoc cref="IDDictionary{T1,T2}" />
    /// <inheritdoc cref="IEntity" />
    /// <summary>
    /// The dynamic persistent database object that holds the configuration data
    /// </summary>
    [Database]
    public class Config : DDictionary, IDDictionary<Config, ConfigKeyValuePair>, IEntity
    {
        private const string All = "SELECT t FROM StarConfig.Config t";

        static Config() => DynamitConfig.Init();

        /// <summary>
        /// Returns the configuration instance used in the current application
        /// </summary>
        public static Config Instance => Db.SQL<Config>(All).FirstOrDefault();

        /// <summary>
        /// Creates a new configuration instance from the path to either a JSON or XML configuration file, or
        /// the name of an environment variable to use to retrieve the path. Environment variables should be
        /// surrounded by '%' characters, for example '%myVariable%'.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Config Create(string path = "%StarConfigPath%")
        {
            if (Path.IsPathRooted(path))
                return Setup(path);
            if (path.StartsWith("%"))
            {
                var environmentVariable = path.Replace("%", "");
                path = Environment.GetEnvironmentVariable(environmentVariable);
                if (path == null)
                {
                    throw new ArgumentException($"Could not find the environment variable '{environmentVariable}' on this computer. " +
                                                "Please add a path to a StarConfig configuration file as value to this environment variable.");
                }
                return Setup(path);
            }
            throw new ArgumentException($"Invalid configuration path '{path}'. The path parameter given in Configuration.Create() must " +
                                        "refer to an environment variable using the following syntax: %<variable name>%, or be an " +
                                        "absolute path to a JSON or XML configuration file.");
        }

        private static Config Setup(string path)
        {
            JObject configToken;
            using (var stream = File.OpenRead(path))
            using (var streamReader = new StreamReader(stream))
            {
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".json":
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            configToken = JToken.Load(jsonReader) as JObject;
                            if (configToken == null)
                                throw new Exception("Invalid JSON file syntax. JSON configuration files must " +
                                                    "contain a single JSON object");
                            break;
                        }
                    case ".xml":
                        var document = new XmlDocument();
                        document.Load(streamReader);
                        var jsonstring = JsonConvert.SerializeXmlNode(document, Formatting.None, false);
                        var token = JObject.Parse(jsonstring)["config"] as JObject ??
                                    throw new Exception("Invalid XML file syntax. XML configuration files must " +
                                                        "contain a root 'config' node.");
                        configToken = new JObject();
                        foreach (var item in token)
                        {
                            JToken resolveValueType()
                            {
                                switch (item.Value.Type)
                                {
                                    case JTokenType.Integer: return item.Value.Value<long>();
                                    case JTokenType.Float: return item.Value.Value<decimal>();
                                    case JTokenType.Boolean: return item.Value.Value<bool>();
                                    case JTokenType.Null: return null;
                                    case JTokenType.Date: return item.Value.Value<DateTime>();
                                }

                                switch (item.Value.ToObject<string>())
                                {
                                    case "true":
                                    case "TRUE":
                                    case "True": return true;
                                    case "false":
                                    case "FALSE":
                                    case "False": return false;
                                    case var dt when DateTime.TryParseExact(dt, "O", null, AssumeLocal, out var dateTime): return dateTime;
                                    case var nr when long.TryParse(nr, out var number): return number;
                                    case var dm when decimal.TryParse(dm, out var @decimal): return @decimal;
                                    case var str: return str;
                                }
                            }

                            configToken[item.Key] = resolveValueType();
                        }

                        break;
                    case var other: throw new Exception($"Unknown or unsupported file extension '{other}'");
                }
            }
            Config config = null;
            Db.TransactAsync(() =>
            {
                foreach (var obj in Db.SQL<Config>(All))
                    obj.Delete();
                config = configToken.ToObject<Config>();
            });
            return config;
        }

        /// <inheritdoc />
        public ConfigKeyValuePair NewKeyPair(Config dict, string key, object value)
        {
            return new ConfigKeyValuePair(dict, key, value);
        }
    }
}
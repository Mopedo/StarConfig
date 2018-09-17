using System;
using System.IO;
using System.Xml;
using Dynamit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Starcounter;

namespace StarConfig
{
    [Database]
    public class Configuration : DDictionary, IDDictionary<Configuration, ConfigurationKeyValuePair>
    {
        public static Configuration Create(string path = "%StarConfigPath%", string noFileFoundErrorMessage = null)
        {
            if (Path.IsPathRooted(path))
                return Setup(path);
            if (path.StartsWith("%"))
            {
                var environmentVariable = path.Replace("%", "");
                path = Environment.GetEnvironmentVariable(environmentVariable);
                if (path == null)
                {
                    throw new ArgumentException(noFileFoundErrorMessage ??
                                                $"Could not find the environment variable '{environmentVariable}' on this computer. " +
                                                "Please add a path to a StarConfig configuration file as value to this environment variable.");
                }
                return Setup(path);
            }
            throw new ArgumentException($"Invalid configuration path '{path}'. The path parameter given in Configuration.Create() must " +
                                        "refer to an environment variable using the following syntax: %<variable name>%, or be an " +
                                        "absolute path to a JSON or XML configuration file.");
        }

        private static Configuration Setup(string path)
        {
            JToken configurationToken;
            using (var stream = File.OpenRead(path))
            using (var streamReader = new StreamReader(stream))
            {
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case "json":
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            configurationToken = JToken.Load(jsonReader);
                            if (!(configurationToken is JObject))
                                throw new Exception("Invalid JSON file syntax. JSON configuration files must " +
                                                    "contain a single JSON object");
                            break;
                        }
                    case "xml":
                        var document = new XmlDocument();
                        document.Load(streamReader);
                        var jsonstring = JsonConvert.SerializeXmlNode(document);
                        configurationToken = JObject.Parse(jsonstring)["config"];
                        if (configurationToken == null)
                            throw new Exception("Invalid XML file syntax. XML configuration files must " +
                                                "contain a root 'config' node.");
                        break;
                    case var other: throw new Exception($"Unknown or unsupported file extension '{other}'");
                }
            }
            Configuration configuration = null;
            Db.TransactAsync(() =>
            {
                foreach (var obj in Db.SQL<Configuration>("SELECT t FROM StarConfig.Configuration t"))
                    obj.Delete();
                configuration = configurationToken.ToObject<Configuration>();
            });
            return configuration;
        }

        ConfigurationKeyValuePair IDDictionary<Configuration, ConfigurationKeyValuePair>.NewKeyPair(Configuration dict, string key, object value)
        {
            return new ConfigurationKeyValuePair(dict, key, value);
        }
    }
}
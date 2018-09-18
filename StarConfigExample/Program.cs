using System.Net;
using Newtonsoft.Json;
using Starcounter;

namespace StarConfigExample
{
    public class Program
    {
        public static void Main()
        {
            // Start by creating a configuration file in some folder on the computer. It can be either 
            // an XML file or a JSON file. XML files should have a root 'config' node. JSON files should
            // include a single object. Strings, integers, floats, booleans and datetimes (ISO 8601 strings) are
            // accepted. XML values are parsed to the appropriate data types.

            // There are two ways to instruct StarConfig to use a certain config file, either we assign a path
            // to that file in the call to StarConfig.Config.Create(), or we assign it to an environment variable
            // (StarConfig uses 'StarConfigPath' by default, but any environment variable can be used if the proper
            // syntax is used). Environment variables should be surrounded by '%' characters, for example '%myVariable%'.

            var applicationFolder = Application.Current.WorkingDirectory;
            var configPath = applicationFolder + "/Config.xml";
            // var configPath = applicationFolder + "/Config.json";
            
            // Use StarConfig.Config.Create() to create a new configuration. Configurations are dynamic persistent
            // Starcounter data objects (implemented using the Dynamit NuGet package).

            StarConfig.Config.Create(configPath);
            // StarConfig.Config.Create("%myVariable%");

            Handle.GET("/config", () =>
            {
                // Use Starconfig.Config.Instance to access the configuration
                var config = StarConfig.Config.Instance;
                return JsonConvert.SerializeObject(config, Formatting.Indented);
            });

            // We can use C# dynamic syntax to access the configuration

            dynamic configuration = StarConfig.Config.Instance;
            Db.TransactAsync(() =>
            {
                configuration.ApplicationFolder = applicationFolder;
                configuration.UdpPort = configuration.HttpPort + 1000;
            });

            // To reload the configuration file, simply delete the configuration and create it again

            Handle.DELETE("/config", () =>
            {
                // Use Starconfig.Config.Instance to access the configuration
                var config = StarConfig.Config.Instance;
                Db.TransactAsync(() =>
                {
                    config.Delete();
                    StarConfig.Config.Create(configPath);
                });
                return HttpStatusCode.OK;
            });
        }
    }
}
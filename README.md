# StarConfig

StarConfig is a simple open-source library for creating and accessing dynamic persistent configurations in a Starcounter applications, that is read from XML or JSON files on the local computer. The persistent storage of the configuration is implemented using [Dynamit](https://github.com/Mopedo/Dynamit).

StarConfig is distributed as a [.NET package on NuGet](https://www.nuget.org/packages/StarConfig), and an easy way to install it in an active Visual Studio project is by entering the following into the NuGet Package Manager console:

```
Install-Package StarConfig
```

## Tutorial

> See the StarConfig/StarConfigExample project for details and code examples

Start by creating a configuration file in some folder on the computer. It can be either an XML file or a JSON file. XML files should have a root `config` node. JSON files should include a single object. Strings, integers, floats, booleans and datetimes (ISO 8601 strings) are accepted. XML values are parsed to the appropriate data types.

There are two ways to instruct StarConfig to use a certain config file, either we assign a path to that file in the call to `StarConfig.Config.Create()`, or we assign it to an environment variable (StarConfig uses `StarConfigPath` by default, but any environment variable can be used). The environment variable is then included in the call to `StarConfig.Config.Create()`. Environment variables should be surrounded by `'%'` characters, for example `%myVariable%`.


```csharp
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
```

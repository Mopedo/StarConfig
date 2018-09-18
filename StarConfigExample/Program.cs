using Newtonsoft.Json;
using Starcounter;

namespace StarConfigExample
{
    public class Program
    {
        public static void Main()
        {
            StarConfig.Config.Create(Application.Current.WorkingDirectory + "/Config.xml");
            //StarConfig.Config.Create(Application.Current.WorkingDirectory + "/Config.json");
            
            Handle.GET("/config", () =>
            {
                var config = StarConfig.Config.Instance;
                return JsonConvert.SerializeObject(config, Formatting.Indented);
            });
        }
    }
}
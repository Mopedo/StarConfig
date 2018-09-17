using System;
using Newtonsoft.Json;
using Starcounter;

namespace StarConfigExample
{
    public class Program
    {
        public static void Main()
        {
            
            Handle.GET("/config", () =>
            {
                var configuration = Configuration.Current
                return JsonConvert.SerializeObject(Db.SQ);
            });
        }
    }
}
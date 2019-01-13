using System;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Fclp;
using Newtonsoft.Json;

namespace Lightning.Metrics.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var configPath = string.Empty;
            var p = new FluentCommandLineParser();
            p.Setup<string>("configPath")
                .Callback(value => configPath = value)
                .Required()
                .WithDescription("The configuration file's path. E.g. metrics.json");

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine(result.ErrorText);
                Environment.Exit(1);
            }

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"The {nameof(configPath)} {configPath} does not exist");
                Environment.Exit(1);
            }

            MetricsConfiguration config = null;
            try
            {
                config = JsonConvert.DeserializeObject<MetricsConfiguration>(File.ReadAllText(configPath));
                config.Validate();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            try
            {
                var client = new MetricsClient(config);

                client.Start().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
            
        }
    }
}

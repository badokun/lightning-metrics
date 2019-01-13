using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Lightning.Metrics.App
{
    class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);
        
        [FileExists]
        [Option("--configPath <FILE>", Description = "The metrics.json configuration path")]
        [Required]
        public string ConfigPath { get; }

        [Option("--test-influxDb", Description = "Test connectivity to the InfluxDb")]
        public bool TestInfluxDb { get; }

        [Option("--test-lndApi", Description = "Test connectivity to the Lnd Rest Api")]
        public bool TestLndApi { get; }

        private void OnExecute()
        {
            MetricsConfiguration config = null;
            try
            {
                config = JsonConvert.DeserializeObject<MetricsConfiguration>(File.ReadAllText(ConfigPath));
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
                if (TestInfluxDb)
                {
                    client.TestInfluxDb();
                }
                else if (TestLndApi)
                {
                    client.TestLndApi();
                }
                else
                {
                    client.Start().Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }
    }
}

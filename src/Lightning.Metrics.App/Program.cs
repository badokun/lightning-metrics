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


        [Option("--influxDbUri", Description = "The InfluxDb Uri. E.g. http://192.168.1.40:8086")]
        [Required]
        public Uri InfluxDbUri { get; }

        [Option("--network", Description = "The bitcoin network. TestNet or Main")]
        [Required]
        public Network Network { get; } = Network.TestNet;

        [Option("--lndRestApiUri", Description = "The Lnd Rest Api Uri. E.g https://192.168.1.40:8080")]
        [Required]
        public Uri LndRestApiUri { get; }

        [Option("--lndRestApiAllowInsecure", Description = "Allow Insecure Requests to the Lnd Rest Api. Defaults to true")]
        public bool LndRestApiAllowInsecure { get; } = true;

        [Option("--interval", Description = "The interval in seconds to request metrics. Defaults to 10")]
        public int IntervalSeconds { get; } = 10;

        [Option("--influxDbName", Description = "The InfluxDb database name. Defaults to telegraf")]
        public string InfluxDbName { get; } = "telegraf";

        [Option("--metricPrefix", Description = "Prefix all metrics pushed into the InfluxDb. Defaults to lightning")]
        public string MetricPrefix { get; } = "lightning";
        

        [Option("--test-influxDb", Description = "Test connectivity to the InfluxDb")]
        public bool TestInfluxDb { get; }

        [Option("--test-lndApi", Description = "Test connectivity to the Lnd Rest Api")]
        public bool TestLndApi { get; }

        private void OnExecute()
        {
            MetricsConfiguration config = null;
            try
            {
                config = new MetricsConfiguration()
                {
                    InfluxDbUri = InfluxDbUri,
                    Network = Network,
                    LndRestApiUri = LndRestApiUri,
                    LndRestApiAllowInsecure = LndRestApiAllowInsecure,
                    IntervalSeconds = IntervalSeconds,
                    InfluxDbName = InfluxDbName,
                    MetricPrefix = MetricPrefix
                };

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

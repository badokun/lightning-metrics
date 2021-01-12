using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace Lightning.Metrics.App
{
    class Program
    {
        public static async Task<int> Main(string[] args) => await CommandLineApplication.ExecuteAsync<Program>(args);

        [Option("--influxDbUri", Description = "The InfluxDb Uri. E.g. http://192.168.1.40:8086")]
        [Required]
        public Uri InfluxDbUri { get; }

        [Option("--network", Description = "The bitcoin network. TestNet or MainNet")]
        [Required]
        public Network Network { get; } = Network.TestNet;

        [Option("--lndRestApiUri", Description = "The Lnd Rest Api Uri. E.g https://192.168.1.40:8080")]
        [Required]
        public Uri LndRestApiUri { get; }

        [Option("--macaroonHex", Description = "The hex string of the admin.macaroon file. See README.md on how to extract this value")]
        [Required]
        public string MacaroonHex { get; }

        [Option("--certThumbprintHex", Description = "The hex string of the tls.cert. See README.md on how to extract this value")]
        [Required]
        public string CertThumbprintHex { get; }


        [Option("--interval", Description = "The interval in seconds to request metrics. Defaults to 10")]
        public int IntervalSeconds { get; } = 10;

        [Option("--influxDbName", Description = "The InfluxDb database name. Defaults to telegraf")]
        public string InfluxDbName { get; } = "telegraf";

        [Option("--metricPrefix", Description = "Prefix all metrics pushed into the InfluxDb. Defaults to lightning")]
        public string MetricPrefix { get; } = "lightning";

        [Option("--use-mempool", Description = "https://github.com/mempool/mempool By default it is disabled.")]
        public bool UseMempoolBackend { get; } = false;

        [Option("--mempoolApiUri", Description = "The mempool Rest Api Uri. Defaults to https://mempool.space/api/v1")]
        public string MempoolApiUri { get; } = "https://mempool.space/api/v1";


        [Option("--test-influxDb", Description = "Test connectivity to the InfluxDb")]
        public bool TestInfluxDb { get; }

        [Option("--test-lndApi", Description = "Test connectivity to the Lnd Rest Api")]
        public bool TestLndApi { get; }

        private async Task OnExecuteAsync(CancellationToken ct)
        {
            MetricsConfiguration config = null;
            try
            {
                config = new MetricsConfiguration()
                {
                    InfluxDbUri = InfluxDbUri,
                    Network = Network,
                    LndRestApiUri = LndRestApiUri,
                    MacaroonHex = MacaroonHex,
                    CertThumbprintHex = CertThumbprintHex,
                    IntervalSeconds = IntervalSeconds,
                    InfluxDbName = InfluxDbName,
                    MetricPrefix = MetricPrefix,
                    UseMempoolBackend = UseMempoolBackend,
                    MempoolApiUri = MempoolApiUri
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
                if (this.TestInfluxDb)
                {
                    client.TestInfluxDb();
                }
                else if (this.TestLndApi)
                {
                    client.TestLndApi();
                }
                else
                {
                    var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                    await client.Start(version, ct).ConfigureAwait(false);
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

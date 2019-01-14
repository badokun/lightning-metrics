using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;
using Lightning.Metrics.MetricConverters;

namespace Lightning.Metrics
{
    public class MetricsClient
    {
        private readonly MetricsConfiguration _configuration;

        public MetricsClient(MetricsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Start()
        {
            Logger.Debug($"Application starting");
            Logger.Debug($"LND Api  {_configuration.LndRestApiUri}");
            Logger.Debug($"InfluxDb {_configuration.InfluxDbUri}");
            Logger.Debug($"Interval {_configuration.IntervalSeconds} seconds");
            Logger.Debug($"Colleting metrics commencing");

            var client = CreateLndClient();
            
            var metrics = new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName)
                .Batch.AtInterval(TimeSpan.FromSeconds(_configuration.IntervalSeconds))
                .WriteTo.InfluxDB(_configuration.InfluxDbUri, _configuration.InfluxDbName)
                .CreateCollector();

            var walletResponseConverter = new LnrpcWalletBalanceResponseMetric();
            var channelBalanceConverter = new LnrpcChannelBalanceResponseMetric();
            var networkInfoConverter = new LnrpcNetworkInfoMetric();

            while (true)
            {
                try
                {
                    var balance = await client.SwaggerClient.WalletBalanceAsync();
                    var channelbalance = await client.SwaggerClient.ChannelBalanceAsync();
                    var networkinfo = await client.SwaggerClient.GetNetworkInfoAsync();

                    metrics.Write($"{_configuration.MetricPrefix}_{walletResponseConverter.MetricName}", walletResponseConverter.ToDictionary(balance));
                    metrics.Write($"{_configuration.MetricPrefix}_{channelBalanceConverter.MetricName}", channelBalanceConverter.ToDictionary(channelbalance));
                    metrics.Write($"{_configuration.MetricPrefix}_{networkInfoConverter.MetricName}", networkInfoConverter.ToDictionary(networkinfo));

                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    client = CreateLndClient();
                }
           
                Thread.Sleep(TimeSpan.FromSeconds(_configuration.IntervalSeconds));
            }
        }

        public void TestInfluxDb()
        {
            var metrics = new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName)
                .Batch.AtInterval(TimeSpan.FromSeconds(_configuration.IntervalSeconds))
                .WriteTo.InfluxDB(_configuration.InfluxDbUri, _configuration.InfluxDbName)
                .CreateCollector();
            
            metrics.Write($"{_configuration.MetricPrefix}_influxDbTest", new Dictionary<string, object> { {"test", "1" } });

            Logger.Debug("InfluxDb write operation completed successfully");
        }

        public void TestLndApi()
        {
            var client = CreateLndClient();
            var balanceTest =  client.SwaggerClient.WalletBalanceAsync();
            balanceTest.Wait();
            Logger.Debug("LndApi test operation completed successfully");
        }

        private LndClient CreateLndClient()
        {
            var restSet = new LndRestSettings()
            {
                AllowInsecure = _configuration.LndRestApiAllowInsecure,
                Uri = _configuration.LndRestApiUri
            };

            return new LndClient(restSet, _configuration.Network == Network.Main ? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
        }
    }


}

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
            Logger.Debug($"Application starting using Lnd Api: {_configuration.LndRestApiUri} and InfluxDb: {_configuration.InfluxDbUri} on a {_configuration.IntervalSeconds} second interval");
            var restSet = new LndRestSettings()
            {
                AllowInsecure = _configuration.LndRestApiAllowInsecure,
                Uri = _configuration.LndRestApiUri
            };

            var client = new LndClient(restSet, _configuration.Network == Network.Main? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
            
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

                    metrics.Write($"{_configuration.MetricPrefix}_balance", walletResponseConverter.ToDictionary(balance));
                    metrics.Write($"{_configuration.MetricPrefix}_channel_balance", channelBalanceConverter.ToDictionary(channelbalance));
                    metrics.Write($"{_configuration.MetricPrefix}_networkinfo", networkInfoConverter.ToDictionary(networkinfo));

                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    client = new LndClient(restSet, _configuration.Network == Network.Main ? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
                }
           
                Thread.Sleep(TimeSpan.FromSeconds(_configuration.IntervalSeconds));
            }
        }
        
    }


}

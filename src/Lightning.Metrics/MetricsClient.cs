using System;
using System.Collections.Generic;
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
            var pendingOpenChannelConverter = new PendingChannelsResponsePendingOpenChannelMetric();
            var pendingForceClosedChannelConverter = new PendingChannelsResponseForceClosedChannelMetrics();
            

            while (true)
            {
                try
                {
                    var balance = await client.SwaggerClient.WalletBalanceAsync();
                    var channelBalance = await client.SwaggerClient.ChannelBalanceAsync();
                    var networkInfo = await client.SwaggerClient.GetNetworkInfoAsync();
                    var pendingChannels = await client.SwaggerClient.PendingChannelsAsync();
                    
                    metrics.Write($"{_configuration.MetricPrefix}_{walletResponseConverter.MetricName}", walletResponseConverter.GetFields(balance));
                    metrics.Write($"{_configuration.MetricPrefix}_{channelBalanceConverter.MetricName}", channelBalanceConverter.GetFields(channelBalance));
                    metrics.Write($"{_configuration.MetricPrefix}_{networkInfoConverter.MetricName}", networkInfoConverter.GetFields(networkInfo));

                    if (pendingChannels.Pending_open_channels != null)
                    {
                        foreach (var pendingOpen in pendingChannels.Pending_open_channels)
                        {
                            metrics.Write(
                                $"{_configuration.MetricPrefix}_{pendingOpenChannelConverter.MetricName}",
                                pendingOpenChannelConverter.GetFields(pendingOpen),
                                pendingOpenChannelConverter.GetTags(pendingOpen));
                        }
                    }

                    if (pendingChannels.Pending_force_closing_channels != null)
                    {
                        foreach (var pendingForceCLose in pendingChannels.Pending_force_closing_channels)
                        {

                            metrics.Write(
                                $"{_configuration.MetricPrefix}_{pendingForceClosedChannelConverter.MetricName}",
                                pendingForceClosedChannelConverter.GetFields(pendingForceCLose),
                                pendingForceClosedChannelConverter.GetTags(pendingForceCLose));

                            var lnrpcPendingHtlc = new LnrpcPendingHtlcMetrics(pendingForceCLose);
                            foreach (var pendingHtlcs in pendingForceCLose.Pending_htlcs)
                            {
                                metrics.Write(
                                    $"{_configuration.MetricPrefix}_{lnrpcPendingHtlc.MetricName}",
                                    lnrpcPendingHtlc.GetFields(pendingHtlcs),
                                    lnrpcPendingHtlc.GetTags(pendingHtlcs));
                            }
                        }
                    }
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
            if (!LightningConnectionString.TryParse(
                $"type=lnd-rest;server={_configuration.LndRestApiUri};macaroon={_configuration.MacaroonHex};certthumbprint={_configuration.CertThumbprintHex}",
                false, out var connectionString))
            {
                throw new ArgumentException("Unable to contruct the connection string");
            }

            Logger.Debug("LndClient connection string created successfully");
            /*
             * LightningConnectionString.TryParse(
                $"type=lnd-rest;server=https://lnd:lnd@127.0.0.1:53280/;macaroon={macaroon};certthumbprint={certthumbprint2}",
                false, out var conn2);
             */

            return (LndClient)LightningClientFactory.CreateClient(
                connectionString, 
                _configuration.Network == Network.Main ? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
            

        }
    }


}

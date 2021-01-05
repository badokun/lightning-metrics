using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;
using Lightning.Metrics.MetricConverters;

namespace Lightning.Metrics
{
    public class MetricsClient
    {
        private readonly MetricsConfiguration configuration;

        public MetricsClient(MetricsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task Start()
        {
            Logger.Debug($"Application starting");
            Logger.Debug($"LND Api  {configuration.LndRestApiUri}");
            Logger.Debug($"InfluxDb {configuration.InfluxDbUri}");
            Logger.Debug($"Interval {configuration.IntervalSeconds} seconds");
            Logger.Debug($"Colleting metrics commencing");

            var client = CreateLndClient();

            var metrics = new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName)
                .Batch.AtInterval(TimeSpan.FromSeconds(configuration.IntervalSeconds))
                .WriteTo.InfluxDB(configuration.InfluxDbUri, configuration.InfluxDbName)
                .CreateCollector();

            var nodeAliasCache = new NodeAliasCache(client);
            var walletResponseConverter = new LnrpcWalletBalanceResponseMetric(configuration, metrics);
            var channelBalanceConverter = new LnrpcChannelBalanceResponseMetric(configuration, metrics);
            var networkInfoConverter = new LnrpcNetworkInfoMetric(configuration, metrics);
            var pendingOpenChannelConverter = new PendingChannelsResponsePendingOpenChannelMetric(configuration, metrics, nodeAliasCache);
            var pendingForceClosedChannelConverter = new PendingChannelsResponseForceClosedChannelMetrics(configuration, metrics);
            var channelMetrics = new LnrpcChannelMetrics(configuration, metrics, nodeAliasCache);

            while (true)
            {
                var waitingTask = Task.Delay(TimeSpan.FromSeconds(configuration.IntervalSeconds));

                try
                {
                    var balance = await client.SwaggerClient.WalletBalanceAsync().ConfigureAwait(false);
                    var channelBalance = await client.SwaggerClient.ChannelBalanceAsync().ConfigureAwait(false);
                    var networkInfo = await client.SwaggerClient.GetNetworkInfoAsync().ConfigureAwait(false);
                    var pendingChannels = await client.SwaggerClient.PendingChannelsAsync().ConfigureAwait(false);
                    var channelList = await client.SwaggerClient.ListChannelsAsync(null, null, null, null).ConfigureAwait(false);

                    await nodeAliasCache.RefreshOnlyIfNecessary(channelList, pendingChannels).ConfigureAwait(false);

                    walletResponseConverter.WriteMetrics(balance);
                    channelBalanceConverter.WriteMetrics(channelBalance);
                    networkInfoConverter.WriteMetrics(networkInfo);
                    channelMetrics.WriteMetrics(channelList);
                    pendingOpenChannelConverter.WriteMetrics(pendingChannels);
                    pendingForceClosedChannelConverter.WriteMetrics(pendingChannels);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    client = CreateLndClient();
                }
                finally
                {
                    await waitingTask.ConfigureAwait(false);
                }
            }
        }

        public void TestInfluxDb()
        {
            var metrics = new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName)
                .Batch.AtInterval(TimeSpan.FromSeconds(configuration.IntervalSeconds))
                .WriteTo.InfluxDB(configuration.InfluxDbUri, configuration.InfluxDbName)
                .CreateCollector();

            metrics.Write($"{configuration.MetricPrefix}_influxDbTest", new Dictionary<string, object> { { "test", "1" } });

            Logger.Debug("InfluxDb write operation completed successfully");
        }

        public void TestLndApi()
        {
            var client = CreateLndClient();
            var balanceTest = client.SwaggerClient.WalletBalanceAsync();
            balanceTest.Wait();
            Logger.Debug("LndApi test operation completed successfully");
        }

        private LndClient CreateLndClient()
        {
            if (!LightningConnectionString.TryParse(
                $"type=lnd-rest;server={configuration.LndRestApiUri};macaroon={configuration.MacaroonHex};certthumbprint={configuration.CertThumbprintHex}",
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
                configuration.Network == Network.MainNet ? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
        }
    }
}

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
        private readonly MetricsConfiguration configuration;

        private DateTime lastNetworkInfoPollingTime = DateTime.MinValue;

        public MetricsClient(MetricsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task Start(string version, CancellationToken ct)
        {
            Logger.Debug($"Application v.{version} starting");
            Logger.Debug($"LND Api  {this.configuration.LndRestApiUri}");
            Logger.Debug($"InfluxDb {this.configuration.InfluxDbUri}");
            Logger.Debug($"Interval {this.configuration.IntervalSeconds} seconds");
            Logger.Debug($"Colleting metrics commencing");

            var lndClient = this.CreateLndClient();
            var metrics = this.CreateMetricsCollector();
            var mempoolClient = this.CreateMempoolClientIfEnabled(metrics);

            var nodeAliasCache = new NodeAliasCache(lndClient);
            var walletResponseConverter = new LnrpcWalletBalanceResponseMetric(this.configuration, metrics);
            var channelBalanceConverter = new LnrpcChannelBalanceResponseMetric(this.configuration, metrics);
            var networkInfoConverter = new LnrpcNetworkInfoMetric(this.configuration, metrics);
            var pendingOpenChannelConverter = new PendingChannelsResponsePendingOpenChannelMetric(this.configuration, metrics, nodeAliasCache);
            var pendingForceClosedChannelConverter = new PendingChannelsResponseForceClosedChannelMetrics(this.configuration, metrics);
            var channelMetrics = new LnrpcChannelMetrics(this.configuration, metrics, nodeAliasCache);

            while (!ct.IsCancellationRequested)
            {
                var waitingTask = Task.Delay(TimeSpan.FromSeconds(this.configuration.IntervalSeconds), ct);
                var mempoolTask = mempoolClient?.RequestFeesAsync(ct) ?? Task.CompletedTask;

                try
                {
                    var networkInfo = await this.GetNetworkInfoAfterTenfoldWaitingTime(lndClient, ct).ConfigureAwait(false);
                    var balance = await lndClient.SwaggerClient.WalletBalanceAsync(ct).ConfigureAwait(false);
                    var channelBalance = await lndClient.SwaggerClient.ChannelBalanceAsync(ct).ConfigureAwait(false);
                    var pendingChannels = await lndClient.SwaggerClient.PendingChannelsAsync(ct).ConfigureAwait(false);
                    var channelList = await lndClient.SwaggerClient.ListChannelsAsync(null, null, null, null, ct).ConfigureAwait(false);

                    var refreshTask = nodeAliasCache.RefreshOnlyIfNecessary(channelList, pendingChannels);

                    await Task.WhenAll(mempoolTask, refreshTask).ConfigureAwait(false);

                    walletResponseConverter.WriteMetrics(balance);
                    channelBalanceConverter.WriteMetrics(channelBalance);
                    networkInfoConverter.WriteMetrics(networkInfo);
                    channelMetrics.WriteMetrics(channelList);
                    pendingOpenChannelConverter.WriteMetrics(pendingChannels);
                    pendingForceClosedChannelConverter.WriteMetrics(pendingChannels);
                    mempoolClient?.WriteMetrics();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);

                    if (e.InnerException != null)
                    {
                        Logger.Error(e.InnerException.Message);
                    }

                    lndClient = this.CreateLndClient();
                    metrics = this.CreateMetricsCollector();
                }
                finally
                {
                    await waitingTask.ConfigureAwait(false);
                }
            }
        }

        public void TestInfluxDb()
        {
            var metrics = this.CreateMetricsCollector();
            metrics.Write($"{this.configuration.MetricPrefix}_influxDbTest", new Dictionary<string, object> { { "test", "1" } });
            Logger.Debug("InfluxDb write operation completed successfully");
        }

        public void TestLndApi()
        {
            var client = this.CreateLndClient();
            var balanceTest = client.SwaggerClient.WalletBalanceAsync();
            balanceTest.Wait();
            Logger.Debug("LndApi test operation completed successfully");
        }

        private async Task<LnrpcNetworkInfo> GetNetworkInfoAfterTenfoldWaitingTime(LndClient lndClient, CancellationToken ct)
        {
            if ((DateTime.Now - this.lastNetworkInfoPollingTime).TotalMinutes >= this.configuration.IntervalSeconds)
            {
                this.lastNetworkInfoPollingTime = DateTime.Now;

                return await lndClient.SwaggerClient.GetNetworkInfoAsync(ct).ConfigureAwait(false);
            }

            return null;
        }

        private LndClient CreateLndClient()
        {
            if (!LightningConnectionString.TryParse(
                $"type=lnd-rest;server={this.configuration.LndRestApiUri};macaroon={this.configuration.MacaroonHex};certthumbprint={this.configuration.CertThumbprintHex}",
                false, out var connectionString))
            {
                throw new ArgumentException("Unable to contruct the connection string");
            }

            Logger.Debug("LndClient connection string created successfully");

            return (LndClient)LightningClientFactory.CreateClient(
                connectionString,
                this.configuration.Network == Network.MainNet ? NBitcoin.Network.Main : NBitcoin.Network.TestNet);
        }

        private MetricsCollector CreateMetricsCollector()
        {
            return new CollectorConfiguration()
                .Tag.With("host", Environment.MachineName)
                .Batch.AtInterval(TimeSpan.FromSeconds(this.configuration.IntervalSeconds))
                .WriteTo.InfluxDB(this.configuration.InfluxDbUri, this.configuration.InfluxDbName)
                .CreateCollector();
        }

        private MempoolClient CreateMempoolClientIfEnabled(MetricsCollector metrics)
        {
            return this.configuration.UseMempoolBackend
                ? new MempoolClient(this.configuration, metrics)
                : null;
        }
    }
}

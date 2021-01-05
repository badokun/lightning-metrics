using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcChannelBalanceResponseMetric
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public LnrpcChannelBalanceResponseMetric(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(LnrpcChannelBalanceResponse channelBalance)
        {
            metrics.Write($"{configuration.MetricPrefix}_channel_balance", GetFields(channelBalance));
        }

        private static Dictionary<string, object> GetFields(LnrpcChannelBalanceResponse channelBalance)
        {
            return new Dictionary<string, object>
            {
                {  nameof(channelBalance.Balance).ToLowerInvariant(), channelBalance.Balance.ToLong() },
                {  nameof(channelBalance.Pending_open_balance).ToLowerInvariant(), channelBalance.Pending_open_balance.ToLong() }
            };
        }
    }
}
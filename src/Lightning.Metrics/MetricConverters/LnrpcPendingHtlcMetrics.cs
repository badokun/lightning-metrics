using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcPendingHtlcMetrics
    {
        private const string MetricName = "pending_htlcs";

        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public LnrpcPendingHtlcMetrics(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(PendingChannelsResponseForceClosedChannel parent)
        {
            foreach (var pendingHtlcs in parent.Pending_htlcs)
            {
                metrics.Write($"{configuration.MetricPrefix}_{MetricName}", this.GetFields(pendingHtlcs), this.GetTags(parent));
            }
        }

        private Dictionary<string, object> GetFields(LnrpcPendingHTLC metric)
        {
            return new Dictionary<string, object>
            {
                {  nameof(metric.Amount).ToLowerInvariant(), metric.Amount.ToLong()},
                {  nameof(metric.Stage).ToLowerInvariant(), metric.Stage ?? 0},
                {  nameof(metric.Outpoint).ToLowerInvariant(), metric.Outpoint.ToLong() },
                {  nameof(metric.Blocks_til_maturity).ToLowerInvariant(), metric.Blocks_til_maturity ?? 0 },
                {  nameof(metric.Maturity_height).ToLowerInvariant(), metric.Maturity_height ??0 },
            };
        }

        private Dictionary<string, string> GetTags(PendingChannelsResponseForceClosedChannel parent)
        {
            return new Dictionary<string, string>
            {
                { nameof(parent.Closing_txid).ToLowerInvariant(), parent.Closing_txid.Left(Extensions.TagSize) }
            };
        }
    }
}
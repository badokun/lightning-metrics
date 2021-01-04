using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class PendingChannelsResponsePendingOpenChannelMetric
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public PendingChannelsResponsePendingOpenChannelMetric(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(LnrpcPendingChannelsResponse pendingChannelsResponse)
        {
            if (pendingChannelsResponse.Pending_open_channels != null)
            {
                foreach (var pendingOpen in pendingChannelsResponse.Pending_open_channels)
                {
                    metrics.Write($"{configuration.MetricPrefix}_pending_open_channels", GetFields(pendingOpen), GetTags(pendingOpen));
                }
            }
        }

        private static Dictionary<string, object> GetFields(PendingChannelsResponsePendingOpenChannel pendingOpenChannel)
        {
            return new Dictionary<string, object>
            {
                {  nameof(pendingOpenChannel.Channel.Capacity).ToLowerInvariant(), pendingOpenChannel.Channel.Capacity.ToLong()},
                {  nameof(pendingOpenChannel.Channel.Remote_balance).ToLowerInvariant(), pendingOpenChannel.Channel.Remote_balance.ToLong() },
                {  nameof(pendingOpenChannel.Channel.Local_balance).ToLowerInvariant(), pendingOpenChannel.Channel.Local_balance.ToLong() },
                {  nameof(pendingOpenChannel.Commit_fee).ToLowerInvariant(), pendingOpenChannel.Commit_fee.ToLong() },
                {  nameof(pendingOpenChannel.Commit_weight).ToLowerInvariant(), pendingOpenChannel.Commit_weight.ToLong() },
                {  nameof(pendingOpenChannel.Fee_per_kw).ToLowerInvariant(), pendingOpenChannel.Fee_per_kw.ToLong() }
            };
        }

        private static Dictionary<string, string> GetTags(PendingChannelsResponsePendingOpenChannel metric)
        {
            return new Dictionary<string, string>
            {
                { nameof(metric.Channel.Remote_node_pub).ToLowerInvariant(), metric.Channel.Remote_node_pub.Left(Extensions.TagSize) },
                { nameof(metric.Channel.Channel_point).ToLowerInvariant(), metric.Channel.Channel_point.Left(Extensions.TagSize) }
            };
        }
    }
}
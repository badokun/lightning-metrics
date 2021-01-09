using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class PendingChannelsResponseForceClosedChannelMetrics
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public PendingChannelsResponseForceClosedChannelMetrics(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(LnrpcPendingChannelsResponse pendingChannelsResponse)
        {
            if (pendingChannelsResponse.Pending_force_closing_channels != null)
            {
                foreach (var pendingForceCLose in pendingChannelsResponse.Pending_force_closing_channels)
                {
                    this.metrics.Write($"{this.configuration.MetricPrefix}_forced_closed_channels", GetFields(pendingForceCLose), GetTags(pendingForceCLose));

                    new LnrpcPendingHtlcMetrics(this.configuration, this.metrics).WriteMetrics(pendingForceCLose);
                }
            }
        }

        private static Dictionary<string, object> GetFields(PendingChannelsResponseForceClosedChannel metric)
        {
            return new Dictionary<string, object>
            {
                {  nameof(metric.Channel.Capacity).ToLowerInvariant(), metric.Channel.Capacity.ToLong()},
                {  nameof(metric.Channel.Remote_balance).ToLowerInvariant(), metric.Channel.Remote_balance.ToLong() },
                {  nameof(metric.Channel.Local_balance).ToLowerInvariant(), metric.Channel.Local_balance.ToLong() },

                {  nameof(metric.Blocks_til_maturity).ToLowerInvariant(), metric.Blocks_til_maturity ?? 0 },
                {  nameof(metric.Limbo_balance).ToLowerInvariant(), metric.Limbo_balance.ToLong() },
                {  nameof(metric.Maturity_height).ToLowerInvariant(), metric.Maturity_height ??0 },
                {  nameof(metric.Recovered_balance).ToLowerInvariant(), metric.Recovered_balance.ToLong() }
            };
        }

        private static Dictionary<string, string> GetTags(PendingChannelsResponseForceClosedChannel metric)
        {
            return new Dictionary<string, string>
            {
                { nameof(metric.Channel.Remote_node_pub).ToLowerInvariant(), metric.Channel.Remote_node_pub.Left(Extensions.TagSize) },
                { nameof(metric.Channel.Channel_point).ToLowerInvariant(), metric.Channel.Channel_point.Left(Extensions.TagSize) }
            };
        }
    }
}
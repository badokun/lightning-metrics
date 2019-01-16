using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class PendingChannelsResponsePendingOpenChannelMetric : IMetricConverterWithTags<PendingChannelsResponsePendingOpenChannel>
    {
        
        public string MetricName => "pending_open_channels";


        public Dictionary<string, object> GetFields(PendingChannelsResponsePendingOpenChannel pendingOpenChannel)
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

        public Dictionary<string, string> GetTags(PendingChannelsResponsePendingOpenChannel metric)
        {
            return new Dictionary<string, string>
            {
                { nameof(metric.Channel.Remote_node_pub).ToLowerInvariant(), metric.Channel.Remote_node_pub.Left(Extensions.TagSize) },
                { nameof(metric.Channel.Channel_point).ToLowerInvariant(), metric.Channel.Channel_point.Left(Extensions.TagSize) }
            };
        }
    }
}
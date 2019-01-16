using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcPendingHtlcMetrics : IMetricConverterWithTags<LnrpcPendingHTLC>
    {
        private readonly PendingChannelsResponseForceClosedChannel _parent;
        public string MetricName => "pending_htlcs";

        public LnrpcPendingHtlcMetrics(PendingChannelsResponseForceClosedChannel parent)
        {
            _parent = parent;
        }
        public Dictionary<string, object> GetFields(LnrpcPendingHTLC metric)
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

        public Dictionary<string, string> GetTags(LnrpcPendingHTLC metric)
        {
            return new Dictionary<string, string>
            {
                { nameof(_parent.Closing_txid).ToLowerInvariant(), _parent.Closing_txid.Left(Extensions.TagSize) }
            };
        }
    }
}
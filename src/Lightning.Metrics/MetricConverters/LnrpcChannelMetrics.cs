using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcChannelMetrics: IMetricConverterWithTags<LnrpcChannel>
    {
        public string MetricName => "list_channels";
        public Dictionary<string, object> GetFields(LnrpcChannel metric)
        {
            return new Dictionary<string, object>
            {
                { nameof(metric.Capacity).ToLowerInvariant(), metric.Capacity.ToLong() },
                { nameof(metric.Local_balance).ToLowerInvariant(), metric.Local_balance.ToLong() },
                { nameof(metric.Remote_balance).ToLowerInvariant(), metric.Remote_balance.ToLong() },
                { nameof(metric.Unsettled_balance).ToLowerInvariant(), metric.Unsettled_balance.ToLong() },

                { nameof(metric.Total_satoshis_received).ToLowerInvariant(), metric.Total_satoshis_received.ToLong() },
                { nameof(metric.Total_satoshis_sent).ToLowerInvariant(), metric.Total_satoshis_sent.ToLong() }
            };
        }

        public Dictionary<string, string> GetTags(LnrpcChannel metric)
        {
            return new Dictionary<string, string>
            {
                { nameof(metric.Remote_pubkey).ToLowerInvariant(), metric.Remote_pubkey.Left(Extensions.TagSize) },
                { nameof(metric.Channel_point).ToLowerInvariant(), metric.Channel_point.Left(Extensions.TagSize) }
            };
        }
    }
}
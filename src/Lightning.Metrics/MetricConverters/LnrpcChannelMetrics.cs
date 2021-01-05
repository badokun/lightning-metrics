using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcChannelMetrics
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;
        private readonly NodeAliasCache nodeAliasCache;

        public LnrpcChannelMetrics(MetricsConfiguration configuration, MetricsCollector metrics, NodeAliasCache nodeAliasCache)
        {
            this.configuration = configuration;
            this.metrics = metrics;
            this.nodeAliasCache = nodeAliasCache;
        }

        public void WriteMetrics(LnrpcListChannelsResponse listChannelsResponse)
        {
            if (listChannelsResponse?.Channels != null)
            {
                foreach (var channel in listChannelsResponse.Channels)
                {
                    metrics.Write($"{configuration.MetricPrefix}_list_channels", GetFields(channel), this.GetTags(channel));
                }
            }
        }

        private static Dictionary<string, object> GetFields(LnrpcChannel metric)
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

        private Dictionary<string, string> GetTags(LnrpcChannel metric)
        {
            return new Dictionary<string, string>
            {
                { "remote_alias", this.nodeAliasCache.GetNodeAlias(metric.Remote_pubkey) }
            };
        }
    }
}
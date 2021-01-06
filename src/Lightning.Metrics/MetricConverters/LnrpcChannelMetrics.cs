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
                    var nodeAlias = this.nodeAliasCache.GetNodeAlias(channel.Remote_pubkey);
                    metrics.Write($"{configuration.MetricPrefix}_list_channels", GetFields(channel), GetTags(nodeAlias));
                }
            }
        }

        private static Dictionary<string, object> GetFields(LnrpcChannel metric)
        {
            return new Dictionary<string, object>
            {
                { nameof(metric.Active).ToLowerInvariant(), metric.Active.ToInt() },
                { nameof(metric.Capacity).ToLowerInvariant(), metric.Capacity.ToLong() },
                { nameof(metric.Local_balance).ToLowerInvariant(), metric.Local_balance.ToLong() },
                { nameof(metric.Remote_balance).ToLowerInvariant(), metric.Remote_balance.ToLong() },
                { nameof(metric.Unsettled_balance).ToLowerInvariant(), metric.Unsettled_balance.ToLong() },
                { nameof(metric.Total_satoshis_received).ToLowerInvariant(), metric.Total_satoshis_received.ToLong() },
                { nameof(metric.Total_satoshis_sent).ToLowerInvariant(), metric.Total_satoshis_sent.ToLong() },
                { nameof(metric.Commit_fee).ToLowerInvariant(), metric.Commit_fee.ToLong() },
                { nameof(metric.Commit_weight).ToLowerInvariant(), metric.Commit_weight.ToLong() },
                { nameof(metric.Fee_per_kw).ToLowerInvariant(), metric.Fee_per_kw.ToLong() },
                { nameof(metric.Num_updates).ToLowerInvariant(), metric.Num_updates.ToLong() }
            };
        }

        private static Dictionary<string, string> GetTags(string nodeAlias)
        {
            return new Dictionary<string, string>
            {
                { "remote_alias", nodeAlias }
            };
        }
    }
}
using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcNetworkInfoMetric
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public LnrpcNetworkInfoMetric(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(LnrpcNetworkInfo networkInfo)
        {
            metrics.Write($"{configuration.MetricPrefix}_networkinfo", GetFields(networkInfo));
        }

        private static Dictionary<string, object> GetFields(LnrpcNetworkInfo networkInfo)
        {
            return new Dictionary<string, object>
            {
                {  nameof(networkInfo.Max_channel_size).ToLowerInvariant(), networkInfo.Max_channel_size.ToLong() },
                {  nameof(networkInfo.Min_channel_size).ToLowerInvariant(), networkInfo.Min_channel_size.ToLong() },
                {  nameof(networkInfo.Total_network_capacity).ToLowerInvariant(), networkInfo.Total_network_capacity.ToLong()},

                {  nameof(networkInfo.Avg_channel_size).ToLowerInvariant(), networkInfo.Avg_channel_size ?? 0 },
                {  nameof(networkInfo.Avg_out_degree).ToLowerInvariant(), networkInfo.Avg_out_degree ?? 0 },
                {  nameof(networkInfo.Num_channels).ToLowerInvariant(), networkInfo.Num_channels ?? 0 },
                {  nameof(networkInfo.Num_nodes).ToLowerInvariant(), networkInfo.Num_nodes ?? 0 },
                {  nameof(networkInfo.Graph_diameter).ToLowerInvariant(), networkInfo.Graph_diameter ?? 0 },
                {  nameof(networkInfo.Max_out_degree).ToLowerInvariant(), networkInfo.Max_out_degree ?? 0 }
            };
        }
    }
}
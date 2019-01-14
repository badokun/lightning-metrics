using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{ 
    public class LnrpcNetworkInfoMetric: IMetricConverter<LnrpcNetworkInfo>
    {
        public string MetricName => "networkinfo";

        public Dictionary<string, object> ToDictionary(LnrpcNetworkInfo networkinfo)
        {
            return new Dictionary<string, object>
            {
                {  nameof(networkinfo.Max_channel_size).ToLowerInvariant(), networkinfo.Max_channel_size != null ? long.Parse(networkinfo.Max_channel_size): 0 },
                {  nameof(networkinfo.Min_channel_size).ToLowerInvariant(), networkinfo.Min_channel_size != null ? long.Parse(networkinfo.Min_channel_size): 0 },
                {  nameof(networkinfo.Total_network_capacity).ToLowerInvariant(), networkinfo.Total_network_capacity != null ? long.Parse(networkinfo.Total_network_capacity): 0 },

                {  nameof(networkinfo.Avg_channel_size).ToLowerInvariant(), networkinfo.Avg_channel_size ?? 0 },
                {  nameof(networkinfo.Avg_out_degree).ToLowerInvariant(), networkinfo.Avg_out_degree ?? 0 },
                {  nameof(networkinfo.Num_channels).ToLowerInvariant(), networkinfo.Num_channels ?? 0 },
                {  nameof(networkinfo.Num_nodes).ToLowerInvariant(), networkinfo.Num_nodes ?? 0 },
                {  nameof(networkinfo.Graph_diameter).ToLowerInvariant(), networkinfo.Graph_diameter ?? 0 },
                {  nameof(networkinfo.Max_out_degree).ToLowerInvariant(), networkinfo.Max_out_degree ?? 0 }
            };
        }
    }

    //
}
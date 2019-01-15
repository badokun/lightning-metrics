using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcChannelBalanceResponseMetric : IMetricConverter<LnrpcChannelBalanceResponse>
    {
        public string MetricName => "channel_balance";

        public Dictionary<string, object> GetFields(LnrpcChannelBalanceResponse channelbalance)
        {
            return new Dictionary<string, object>
            {
                {  nameof(channelbalance.Balance).ToLowerInvariant(), channelbalance.Balance.ToLong() },
                {  nameof(channelbalance.Pending_open_balance).ToLowerInvariant(), channelbalance.Pending_open_balance.ToLong() }
            };
        }
    }
}
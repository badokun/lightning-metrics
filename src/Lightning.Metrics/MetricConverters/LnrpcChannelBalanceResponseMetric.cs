using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcChannelBalanceResponseMetric : IMetricConverter<LnrpcChannelBalanceResponse>
    {
        public Dictionary<string, object> ToDictionary(LnrpcChannelBalanceResponse channelbalance)
        {
            return new Dictionary<string, object>
            {
                {  nameof(channelbalance.Balance).ToLowerInvariant(), channelbalance.Balance != null ? long.Parse(channelbalance.Balance): 0 },
                {  nameof(channelbalance.Pending_open_balance).ToLowerInvariant(), channelbalance.Pending_open_balance != null ? long.Parse(channelbalance.Pending_open_balance): 0}
            };
        }
    }
}
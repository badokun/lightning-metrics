using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcWalletBalanceResponseMetric : IMetricConverter<LnrpcWalletBalanceResponse>
    {
        public string MetricName => "balance";

        public Dictionary<string, object> GetFields(LnrpcWalletBalanceResponse balance)
        {
            return new Dictionary<string, object>
            {
                { nameof(balance.Confirmed_balance).ToLowerInvariant(), balance.Confirmed_balance.ToLong() },
                { nameof(balance.Total_balance).ToLowerInvariant(), balance.Total_balance.ToLong() },
                { nameof(balance.Unconfirmed_balance).ToLowerInvariant(), balance.Unconfirmed_balance.ToLong() }
            };
        }
    }
}
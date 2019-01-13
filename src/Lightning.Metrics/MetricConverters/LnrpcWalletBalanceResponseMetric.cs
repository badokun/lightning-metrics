using System.Collections.Generic;
using BTCPayServer.Lightning.LND;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcWalletBalanceResponseMetric : IMetricConverter<LnrpcWalletBalanceResponse>
    {
        public Dictionary<string, object> ToDictionary(LnrpcWalletBalanceResponse balance)
        {
            return new Dictionary<string, object>
            {
                { nameof(balance.Confirmed_balance).ToLowerInvariant(), balance.Confirmed_balance != null ? long.Parse(balance.Confirmed_balance) : 0 },
                { nameof(balance.Total_balance).ToLowerInvariant(), balance.Total_balance != null ? long.Parse(balance.Total_balance) : 0 },
                { nameof(balance.Unconfirmed_balance).ToLowerInvariant(), balance.Unconfirmed_balance != null ? long.Parse(balance.Unconfirmed_balance) : 0
                }
            };
        }
    }
}
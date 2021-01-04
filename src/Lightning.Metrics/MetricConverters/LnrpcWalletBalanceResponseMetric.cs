using System.Collections.Generic;
using BTCPayServer.Lightning.LND;
using InfluxDB.Collector;

namespace Lightning.Metrics.MetricConverters
{
    public class LnrpcWalletBalanceResponseMetric
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        public LnrpcWalletBalanceResponseMetric(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;
        }

        public void WriteMetrics(LnrpcWalletBalanceResponse balance)
        {
            this.metrics.Write($"{configuration.MetricPrefix}_balance", GetFields(balance));
        }

        private static Dictionary<string, object> GetFields(LnrpcWalletBalanceResponse balance)
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
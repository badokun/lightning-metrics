using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Collector;
using RestSharp;

namespace Lightning.Metrics
{
    public class MempoolClient
    {
        private readonly MetricsConfiguration configuration;
        private readonly MetricsCollector metrics;

        private readonly RestClient client;
        private readonly RestRequest feesRequest;

        private IRestResponse<RecommendedFees> feesResponse;

        public MempoolClient(MetricsConfiguration configuration, MetricsCollector metrics)
        {
            this.configuration = configuration;
            this.metrics = metrics;

            this.client = new RestClient(configuration.MempoolApiUri);
            this.feesRequest = new RestRequest("fees/recommended", DataFormat.Json);
        }

        public async Task RequestFeesAsync(CancellationToken ct)
        {
            this.feesResponse = await this.client.ExecuteAsync<RecommendedFees>(this.feesRequest, ct).ConfigureAwait(false);
        }

        public void WriteMetrics()
        {
            if (this.feesResponse.IsSuccessful)
            {
                this.metrics.Write($"{this.configuration.MetricPrefix}_recommended_onchain_fees", GetFields(this.feesResponse.Data));
            }
            else
            {
                Logger.Error("No data could be retrieved from the mempool backend.");
            }
        }

        private static Dictionary<string, object> GetFields(RecommendedFees fees)
        {
            return new Dictionary<string, object>
            {
                {  nameof(fees.FastestFee).ToLowerInvariant(), fees.FastestFee },
                {  nameof(fees.HalfHourFee).ToLowerInvariant(), fees.HalfHourFee },
                {  nameof(fees.HourFee).ToLowerInvariant(), fees.HourFee }
            };
        }
    }

    public class RecommendedFees
    {
        public int FastestFee { get; set; }
        public int HalfHourFee { get; set; }
        public int HourFee { get; set; }
    }
}

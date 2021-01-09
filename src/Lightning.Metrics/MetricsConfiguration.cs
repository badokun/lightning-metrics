using System;

namespace Lightning.Metrics
{
    public enum Network
    {
        TestNet,
        MainNet
    }

    public class MetricsConfiguration
    {
        public Uri InfluxDbUri { get; set; }
        public string InfluxDbName { get; set; }

        public Network Network { get; set; }

        public int IntervalSeconds { get; set; }

        public Uri LndRestApiUri { get; set; }
        public string MetricPrefix { get; set; }
        public string MacaroonHex { get; set; }
        public string CertThumbprintHex { get; set; }

        public bool UseMempoolBackend { get; set; }
        public string MempoolApiUri { get; set; }

        public void Validate()
        {
            const int minInterval = 10;

            if (this.IntervalSeconds < minInterval)
            {
                throw new ArgumentException($"The {nameof(this.IntervalSeconds)} should be greater than {minInterval}");
            }

            if (this.UseMempoolBackend && string.IsNullOrEmpty(this.MempoolApiUri))
            {
                throw new ArgumentException($"The {nameof(this.MempoolApiUri)} must not be null if the mempool backend is enabled");
            }
        }
    }
}

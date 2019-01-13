﻿using System;
using NBitcoin;

namespace Lightning.Metrics
{
    public enum Network
    {
        TestNet,
        Main
    }

    public class MetricsConfiguration
    {
        public Uri InfluxDbUri { get; set; }
        public string InfluxDbName { get; set; }

        public Network Network { get; set; }

        public int IntervalSeconds { get; set; }

        public Uri LndRestApiUri { get; set; }
        public bool LndRestApiAllowInsecure { get; set; }
        public string MetricPrefix { get; set; }

        public void Validate()
        {
            const int minInterval = 10;

            if (IntervalSeconds < minInterval)
            {
                throw new ArgumentException($"The {nameof(IntervalSeconds)} should be greater than {minInterval}");
            }
        }
    }
}
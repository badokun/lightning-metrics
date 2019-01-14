using System.Collections.Generic;

namespace Lightning.Metrics
{
    public interface IMetricConverter<in T>
    {
        string MetricName { get; }
        Dictionary<string, object> ToDictionary(T metric);
    }
}
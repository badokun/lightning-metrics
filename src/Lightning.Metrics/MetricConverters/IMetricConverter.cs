using System.Collections.Generic;

namespace Lightning.Metrics
{
    public interface IMetricConverterWithTags<in T>: IMetricConverter<T>
    {
        Dictionary<string, string> GetTags(T metric);
    }

    public interface IMetricConverter<in T>
    {
        string MetricName { get; }
        Dictionary<string, object> GetFields(T metric);
    }
}
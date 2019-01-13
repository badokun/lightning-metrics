using System.Collections.Generic;

namespace Lightning.Metrics
{
    public interface IMetricConverter<in T>
    {
        Dictionary<string, object> ToDictionary(T metric);
    }
}
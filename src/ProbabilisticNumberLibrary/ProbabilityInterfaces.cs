using System.Numerics;
using System.Reflection.Metadata;

namespace ProbabilisticNumberLibrary;

public interface ProbabilityDistribution<T>
{
    public T Sample();
}

public interface DiscreteDistribution<T> : ProbabilityDistribution<T> { }

public interface UniformDistribution<T> : ProbabilityDistribution<T> { }

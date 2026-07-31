using System.Numerics;
using System.Reflection.Metadata;

namespace ProbabilisticNumberLibrary;

public interface IProbabilityDistribution<T>
{
    public T Sample();

    public Dictionary<T, double> ToDictionary();
    public string ToFormattedString(int indentLevel = 0);
}

public interface IDiscreteDistribution<T> : IProbabilityDistribution<T> { }

public interface IUniformDistribution<T> : IProbabilityDistribution<T> { }

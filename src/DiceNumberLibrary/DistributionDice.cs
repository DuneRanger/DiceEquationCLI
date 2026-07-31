using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public class DistributionDice<T> : IDiceNumber<T>
where T : INumber<T>
{
    private readonly int _rollCount;
    private IDiscreteDistribution<T> _distribution;

    public DistributionDice(IDiscreteDistribution<T> distribution, int rollCount = 1)
    {
        if (rollCount < 0) throw new ArgumentException("Roll count must be greater or equal to 0");
        _rollCount = rollCount;
        _distribution = distribution;
    }

    public T Sample()
    {
        T sum = T.Zero;
        for (int i = 0; i < _rollCount; i++) sum += _distribution.Sample();
        return sum;
    }
    
    public Dictionary<T, double> ToDictionary()
    {
        return ToDiscreteDistribution().ToDictionary();
    }
    
    public IDiscreteDistribution<T> ToDiscreteDistribution()
    {
        var baseDist = ProbabilisticNumber<T>.FromDictionary(_distribution.ToDictionary());
        // essentially an empty distribution
        var sumDist = new ProbabilisticNumber<T>([T.AdditiveIdentity], [1.0]);
        for (int i = 0; i < _rollCount; i++) sumDist += baseDist;
        return sumDist;
    }
    
    public string ToFormattedString(int indentLevel = 0)
    {
        return ToDiscreteDistribution().ToFormattedString(indentLevel);
    }
}
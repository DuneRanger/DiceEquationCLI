using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public class UniformRangeDice : IDiceNumber<long>
{
    private readonly int _rollCount;
    private readonly long _min;
    private readonly long _max;
    private Random _rng = new();

    public UniformRangeDice(long min, long max, int rollCount = 1)
    {
        if (min > max) throw new ArgumentException("Min must be less than or equal to max");
        if (min < 0) throw new ArgumentException("Min must be greater or equal to 0");
        if (rollCount < 0) throw new ArgumentException("Roll count must be greater or equal to 0");
        
        _rollCount = rollCount;
        _min = min;
        _max = max;
    }

    public long Sample()
    {
        long sum = 0;
        for (int i = 0; i < _rollCount; i++) sum += _min + (_rng.NextInt64() % (_max - _min));
        return sum;
    }

    public Dictionary<long, double> ToDictionary()
    {
        var dict = new Dictionary<long, double>();
        for (long i = _min; i <= _max; i++) dict[i] = 1.0 / (_max - _min + 1);
        return dict;
    }
    
    public IDiscreteDistribution<long> ToDiscreteDistribution()
    {
        var baseDist = ProbabilisticNumber<long>.FromUniformRange(_min, _max);
        // essentially an empty distribution (additive identity)
        var sumDist = new ProbabilisticNumber<long>([0], [1.0]);
        for (int i = 0; i < _rollCount; i++) sumDist += baseDist;
        return sumDist;
    }

    public string ToFormattedString(int indentLevel = 0)
    {
        return ToDiscreteDistribution().ToFormattedString(indentLevel);
    }
}
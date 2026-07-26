using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public class DistributionDice<T> : IDiceNumber<T>
where T : INumber<T>
{
    private readonly int _rollCount;
    private ProbabilisticNumber<T> _distribution;

    public DistributionDice(ProbabilisticNumber<T> distribution, int rollCount = 1)
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
    
    public ProbabilisticNumber<long> ToProbabilisticNumber()
    {
        return ProbabilisticNumber<long>.FromUniformRange(_min, _max);
    }
}
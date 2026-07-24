using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public class DiceNumber<T>(int rollCount, ProbabilisticNumber<T> diceDistribution)
    where T : INumber<T>
{
    public int RollCount { get; set; } = rollCount;
    private ProbabilisticNumber<T> _diceDistribution { get; set; } = diceDistribution;
    
    public T Sample()
    {
        T sum = T.Zero;
        for (int i = 0; i < RollCount; ++i) sum += _diceDistribution.Sample();
        return sum;
    }

    static DiceNumber<long> FromFairDice(int rollCount, int diceSides)
    {
        return new DiceNumber<long>(rollCount, ProbabilisticNumber<long>.FromUniformRange(1, (long)diceSides + 1));
    }
}
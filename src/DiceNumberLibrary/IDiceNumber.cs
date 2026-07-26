using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public interface IDiceNumber<TValue> : DiscreteDistribution<TValue> {}
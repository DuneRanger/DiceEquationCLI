using System.Numerics;
using ProbabilisticNumberLibrary;

namespace DiceNumberLibrary;

public interface IDiceNumber<TValue> : IDiscreteDistribution<TValue> where TValue : notnull {}
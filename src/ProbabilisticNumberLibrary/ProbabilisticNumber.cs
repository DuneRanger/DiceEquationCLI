using System.Numerics;
using System.Reflection.Metadata;

namespace ProbabilisticNumberLibrary;

public class ProbabilityException : ApplicationException
{
    public ProbabilityException(string message) : base(message) { }
}

public class ProbabilisticNumber<T> :
    IAdditionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    ISubtractionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    IMultiplyOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    IDivisionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>
    where T : INumber<T>
{
    private T[] _values { get; set; }
    private double[] _probs { get; set; }
    private Random _rng = new();
    
    public long Count => _values.Length;
    public Span<T> ValuesView => _values;

    public ProbabilisticNumber(T[] values, double[] probs)
    {
        if (values.Length != probs.Length) throw new ArgumentException("Values and probabilities must be of equal length");
        _values = values;
        _probs = probs;
    }

    public T Sample()
    {
        double prob = _rng.NextDouble();
        for (int i = 0; i < _probs.Length; ++i)
        {
            prob -= _probs[i];
            if (prob <= 0) return _values[i];
        }
        throw new ProbabilityException("Invalid probability sampled");
    }

    public void Normalize()
    {
        double probSum = _probs.Sum();
        double normalizeFactor = 1 / probSum;
        for (int i = 0; i < _probs.Length; ++i) _probs[i] *= normalizeFactor;
    }

    public void PurgeValues(Predicate<T> predicate)
    {
        List<int> idxToRemove = new();
        for (int i = 0; i < _values.Length; ++i)
        {
            if (predicate(_values[i])) idxToRemove.Add(i);
        }
        var newValues = _values.Where((val, idx) => !idxToRemove.Contains(idx)).ToArray();
        var newProbs = _probs.Where((prob, idx) => !idxToRemove.Contains(idx)).ToArray();
        _values = newValues;
        _probs = newProbs;
    }

    public void ModifyValues(Func<T, T> modifier)
    {
        _values = _values.Select(modifier).ToArray();
    }

    public static ProbabilisticNumber<T> FromDictionary(IDictionary<T, double> dict)
    {
        var values = dict.Keys.ToArray();
        var probs = dict.Values.ToArray();
        return new(values, probs);
    }

    public static ProbabilisticNumber<T> FromUniform(T[] values)
    {
        double prob = 1.0 / values.Length;
        var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i) probs[i] = prob;
        return new(values, probs);
    }

    public static ProbabilisticNumber<int> FromUniformRange(int start, int end)
    {
        var values = new int[end - start];
        var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i)
        {
            values[i] = start + i;
            probs[i] = 1.0 / values.Length;
        }
        return new ProbabilisticNumber<int>(values, probs);
    }
    
    public static ProbabilisticNumber<long> FromUniformRange(long start, long end)
    {
        var values = new long[end - start];
        var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i)
        {
            values[i] = start + i;
            probs[i] = 1.0 / values.Length;
        }
        return new ProbabilisticNumber<long>(values, probs);
    }

    private static ProbabilisticNumber<T> ApplyOperation(Func<T, T, T> operation, ProbabilisticNumber<T> left,
        ProbabilisticNumber<T> right)
    {
        Dictionary<T, double> newProbs = new();
        for (int i = 0; i < left.Count; ++i)
        {
            for (int j = 0; j < right.Count; ++j)
            {
                T newVal = operation(left._values[i], right._values[j]);
                double newProb = left._probs[i] * right._probs[j];
                newProbs.TryGetValue(newVal, out double prob);
                newProbs[newVal] = prob + newProb;
            }
        }
        return FromDictionary(newProbs);
    }
    
    public static ProbabilisticNumber<T> operator +(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 + num2, left, right);
    }
    
    public static ProbabilisticNumber<T> operator -(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 - num2, left, right);
    }
    
    public static ProbabilisticNumber<T> operator *(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 * num2, left, right);
    }
    
    public static ProbabilisticNumber<T> operator /(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 / num2, left, right);
    }
}

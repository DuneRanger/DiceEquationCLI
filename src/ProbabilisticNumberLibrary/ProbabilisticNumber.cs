using System.Numerics;
using System.Reflection.Metadata;

namespace ProbabilisticNumberLibrary;

public class ProbabilisticNumber<T> : IDiscreteDistribution<T>,
    IAdditionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    ISubtractionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    IMultiplyOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>,
    IDivisionOperators<ProbabilisticNumber<T>, ProbabilisticNumber<T>, ProbabilisticNumber<T>>
    where T : INumber<T>
{
	/// <summary>
	/// The values of the probabilistic number.
	/// Their order must correspond to the order of the probabilities in _probs.
	/// </summary>
	private T[] _values { get; set; }
	/// <summary>
	/// The probabilities of the values in _values.
    /// Their order must correspond to the order of the values in _values.
	/// </summary>
	private double[] _probs { get; set; }
    private Random _rng = new();

	/// <summary>
	/// The number of possible outcomes of the probabilistic number.
	/// </summary>
	public long Count => _values.Length;

	/// <summary>
	/// A read only view of the possible outcomes of the probabilistic number.
	/// </summary>
	public Span<T> ValuesView => _values;

	/// <summary>
	/// Creates a new ProbabilisticNumber with the given values and probabilities.
	/// An ArgumentException is thrown if the lengths of the values and probabilities arrays are not equal.
	/// </summary>
	/// <param name="values">The values of the probabilistic number.</param>
	/// <param name="probs">The probabilities of the respective values.</param>
	/// <exception cref="ArgumentException"></exception>
	public ProbabilisticNumber(T[] values, double[] probs)
    {
        if (values.Length != probs.Length) throw new ArgumentException("Values and probabilities must be of equal length");
        _values = values;
        _probs = probs;
    }

	/// <summary>
	/// Samples a value from the probabilistic number according to the probabilities of the values.
	/// If the probabilites do not sum up to 1 and a probability greater than the sum of the probabilities is sampled, a ProbabilityException is thrown.
	/// </summary>
	/// <returns>The sampled value.</returns>
	/// <exception cref="ProbabilityException"></exception>
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

	/// <summary>
	/// Normalizes the probabilities of the probabilistic number so that they sum up to 1.
	/// Expects the sum of probabilities to be greater than 0, otherwise a DivideByZeroException is thrown.
	/// </summary>
	public void Normalize()
    {
        double probSum = _probs.Sum();
        double normalizeFactor = 1 / probSum;
        for (int i = 0; i < _probs.Length; ++i) _probs[i] *= normalizeFactor;
    }

	/// <summary>
	/// Removes values and their respective probabilities from the probabilistic number that satisfy the given predicate.
    /// Note: If the sum of probabilities is less than 1, a ProbabilityException may be thrown when sampling.
	/// </summary>
	/// <param name="predicate">The predicate to test the values against.</param>
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

	/// <summary>
	/// Modifies every possible value of the probabilistic number using the given modifier function.
	/// The order of the values is preserved, and the probabilities remain unchanged.
	/// </summary>
	/// <param name="modifier">The function to apply to each value.</param>
	public void ModifyValues(Func<T, T> modifier)
    {
        _values = _values.Select(modifier).ToArray();
    }

	/// <summary>
	/// Converts the probabilistic number to a dictionary where the keys are the possible values and the values are their respective probabilities.
	/// The output of the dictionary is not normalized, meaning that the sum of the probabilities may not equal 1.
	/// </summary>
	/// <returns>A dictionary mapping values to their probabilities.</returns>
	public Dictionary<T, double> ToDictionary()
    {
        var dict = new Dictionary<T, double>();
        for (int i = 0; i < _values.Length; ++i)
        {
            dict.TryGetValue(_values[i], out double prob);
            dict[_values[i]] = prob + _probs[i];
        }
        return dict;
    }

	/// <summary>
	/// Converts the probabilistic number to a dictionary where the keys are the possible values and the values are their respective probabilities.
	/// If the sum of the probabilities is less than 1, an additional key "error" is added to the dictionary with the value of the remaining probability.
	/// If the sum of probabilities is greater than 1, nothing will happen and the probabilities will not be normalized.
	/// </summary>
	/// <returns>A dictionary mapping values to their probabilities.</returns>
	public Dictionary<string, double> ToDictionaryWithError()
    {
        var dict = new Dictionary<string, double>();
        double errorProb = 1.0;
        for (int i = 0; i < _values.Length; ++i)
        {
            string val = _values[i].ToString()!;
            dict.TryGetValue(val, out double prob);
            dict[val] = prob + _probs[i];
            errorProb -= _probs[i];
        }
        if (errorProb > 1e-10) dict["error"] = errorProb;
        return dict;
    }

	/// <summary>
	/// Returns a formatted string representation of the probabilistic number, with each value and its probability on a new line.
	/// The output is equivalent to the output of ToDictionaryWithError(), but formatted for readability.
	/// <br></br>
	/// Numbers are sorted in ascending order, and the "error" key is always printed first if it exists.
	/// Each value is left-aligned to a constant width, and the probabilities are formatted as percentages.
	/// Small probabilities are written in scientific notation.
	/// <br></br>
    /// The string will contain a newline at the end
	/// </summary>
	/// <param name="indentLevel">The number of tabs to indent each line.</param>
	/// <returns>The formatted string representation of the probabilistic number.</returns>
	public string ToFormattedString(int indentLevel = 0)
    {
        var dict = ToDictionaryWithError();

        // order keys numerically
        List<string> sortedKeys = new();
        if (dict.ContainsKey("error")) sortedKeys.Add("error");
        sortedKeys.AddRange(dict.Keys.Where(key => key != "error").OrderBy(key => double.Parse(key)));

		// orders by probability (descending)
		//var sortedKeys = dict.Keys.OrderBy(key => -dict[key]);
		string str = "";
        int longestKeyLength = sortedKeys.Max(key => key.Length);
		foreach (var key in sortedKeys)
        {
            for (int i = 0; i < indentLevel; ++i) str += "\t";

            if (longestKeyLength < 5) str += $"{key,-4}: ";
            else str += $"{key,-10}: ";

            var prob = dict[key];
            if (prob < 0.001) str += $"{prob * 100:g2}";
            else str += $"{prob * 100:f2}";

			str += "%\n";
		}
        return str;
    }

	/// <summary>
	/// Creates a new ProbabilisticNumber from a dictionary where the keys are the possible values and the values are their respective probabilities.
	/// </summary>
	/// <param name="dict">The dictionary mapping values to their probabilities.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> FromDictionary(IDictionary<T, double> dict)
    {
        var values = dict.Keys.ToArray();
        var probs = dict.Values.ToArray();
        return new(values, probs);
    }

	/// <summary>
	/// Creates a new ProbabilisticNumber with uniform probabilities for the given values, based on their count.
	/// Duplicates are not checked for, so duplicate values will effectively have their probabilities summed together.
	/// </summary>
	/// <param name="values">An array of values which will be assigned uniform probabilities.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> FromUniform(T[] values)
    {
        double prob = 1.0 / values.Length;
        var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i) probs[i] = prob;
        return new(values, probs);
    }

	/// <summary>
	/// Creates a new ProbabilisticNumber with uniform probabilities for the integer values in the range [start, end).
	/// if end is less than or equal to start, an ArgumentException is thrown.
	/// </summary>
	/// <param name="start">The start of the range (inclusive).</param>
	/// <param name="end">The end of the range (exclusive).</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	/// <exception cref="ArgumentException"></exception>
	public static ProbabilisticNumber<int> FromUniformRange(int start, int end)
    {
        if (end <= start) throw new ArgumentException("End must be greater than start");
        var values = new int[end - start];
        var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i)
        {
            values[i] = start + i;
            probs[i] = 1.0 / values.Length;
        }
        return new ProbabilisticNumber<int>(values, probs);
    }

	/// <summary>
	/// Creates a new ProbabilisticNumber with uniform probabilities for the integer values in the range [start, end).
	/// if end is less than or equal to start, an ArgumentException is thrown.
	/// </summary>
	/// <param name="start">The start of the range (inclusive).</param>
	/// <param name="end">The end of the range (exclusive).</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	/// <exception cref="ArgumentException"></exception>
	public static ProbabilisticNumber<long> FromUniformRange(long start, long end)
    {
        if (end <= start) throw new ArgumentException("End must be greater than start");
        var values = new long[end - start];
		var probs = new double[values.Length];
        for (int i = 0; i < probs.Length; ++i)
        {
            values[i] = start + i;
            probs[i] = 1.0 / values.Length;
        }
        return new ProbabilisticNumber<long>(values, probs);
    }

	/// <summary>
	/// Applies a binary operation to two ProbabilisticNumbers and returns a new ProbabilisticNumber representing the result.
	/// The operation is applied to all combinations of values from the left and right ProbabilisticNumbers
    /// and the resulting probabilities are calculated by multiplying the probabilities of the corresponding values.
	/// </summary>
	/// <param name="operation">The binary operation to apply.</param>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
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

	/// <summary>
	/// Defines the addition operator for two ProbabilisticNumbers, returning a new ProbabilisticNumber.
	/// The addition is defined by effectively sampling every possible combination of values from the two ProbabilisticNumbers and summing them,
	/// with the resulting probabilities calculated by multiplying the probabilities of the corresponding values.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> operator +(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 + num2, left, right);
    }

	/// <summary>
	/// Defines the addition operator for two ProbabilisticNumbers, returning a new ProbabilisticNumber.
	/// The addition is defined by effectively sampling every possible combination of values from the two ProbabilisticNumbers and subtracting them,
	/// with the resulting probabilities calculated by multiplying the probabilities of the corresponding values.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> operator -(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 - num2, left, right);
    }

	/// <summary>
	/// Defines the addition operator for two ProbabilisticNumbers, returning a new ProbabilisticNumber.
	/// The addition is defined by effectively sampling every possible combination of values from the two ProbabilisticNumbers and multiplying them,
	/// with the resulting probabilities calculated by multiplying the probabilities of the corresponding values.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> operator *(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 * num2, left, right);
    }

	/// <summary>
	/// Defines the addition operator for two ProbabilisticNumbers, returning a new ProbabilisticNumber.
	/// The addition is defined by effectively sampling every possible combination of values from the two ProbabilisticNumbers and dividing them,
	/// with the resulting probabilities calculated by multiplying the probabilities of the corresponding values.
	/// Note: If any value in the right ProbabilisticNumber is zero and the type T does not support division by zero, a DivideByZeroException may be thrown during the operation.
	/// </summary>
	/// <param name="left">The left operand.</param>
	/// <param name="right">The right operand.</param>
	/// <returns>The new ProbabilisticNumber instance.</returns>
	public static ProbabilisticNumber<T> operator /(ProbabilisticNumber<T> left, ProbabilisticNumber<T> right)
    {
        return ApplyOperation((num1, num2) => num1 / num2, left, right);
    }
}

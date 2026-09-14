using ProbabilisticNumberLibrary;
using DiceNumberLibrary;
using System.Collections.Concurrent;

namespace DiceEquationCLI;
public interface IOperatorNode { }

public abstract class BinaryOperatorNode : BinaryNode, IOperatorNode { }
public abstract class UnaryOperatorNode : UnaryNode, IOperatorNode { }

public class AdditionNode : BinaryOperatorNode
{
    public override double Evaluate() => Left!.Evaluate() + Right!.Evaluate();
	public override ProbabilisticNumber<double> GetDistribution() => Left!.GetDistribution() + Right!.GetDistribution();
}

public class SubtractionNode : BinaryOperatorNode
{
    public override double Evaluate() => Left!.Evaluate() - Right!.Evaluate();
	public override ProbabilisticNumber<double> GetDistribution() => Left!.GetDistribution() - Right!.GetDistribution();
}

public class MultiplicationNode : BinaryOperatorNode
{
    public override double Evaluate() => Left!.Evaluate() * Right!.Evaluate();
	public override ProbabilisticNumber<double> GetDistribution() => Left!.GetDistribution() * Right!.GetDistribution();
}

public class DivisionNode : BinaryOperatorNode
{
    public override double Evaluate() {
        if (Right!.Evaluate() == 0)
        {
            throw new DivideByZeroException("Division by zero is not allowed.");
        }
        return Left!.Evaluate() / Right!.Evaluate();
    }
    public override ProbabilisticNumber<double> GetDistribution()
    {
        var right = Right!.GetDistribution();
        right.PurgeValues(value => value == 0);
		return Left!.GetDistribution() / right;
    }
}

public class DiceSimulationNode : BinaryOperatorNode
{
    // Note: because the left and right values aren't necessarily deterministic,
    // we can't create a single UniformRangeDice and use that for Evaluate.

    Random rng= new Random();

	public override double Evaluate()
    {
        double leftValue = Left!.Evaluate();
        double rightValue = Right!.Evaluate();

        if (leftValue < 0 || Math.Round(leftValue) != leftValue)
		{
			throw new ArgumentException("Invalid dice roll count: Must be a non-negative integer.");
		}

        if (rightValue < 0 || Math.Round(rightValue) != rightValue)
		{
			throw new ArgumentException("Invalid dice sides: Must be a positive integer.");
		}
        if (rightValue >= Int64.MaxValue) throw new ArgumentException($"Invalid dice sides: dice cannot have more than {(double)(Int64.MaxValue - 1)} sides");
        if (rightValue == 0) return 0;

        double result = 0;
        for (int i = 0; i < leftValue; i++)
        {
            checked
            {
                result += rng.NextInt64(1, (long)rightValue + 1);
            }
		}
        return result;
    }
    public override ProbabilisticNumber<double> GetDistribution()
    {
		var leftTask = Task.Run(() => Left!.GetDistribution());
		var rightTask = Task.Run(() => Right!.GetDistribution());

		var leftDistribution = leftTask.Result;
		var rightDistribution = rightTask.Result;

		// Purge invalid values from each distribution (i.e. negative or non-integer values)
		leftDistribution.PurgeValues(value => value < 0 || Math.Round(value) != value || value > Int32.MaxValue);
		rightDistribution.PurgeValues(value => value < 0 || Math.Round(value) != value || value >= Int64.MaxValue);

		ConcurrentDictionary<double, double> concurrentDistributionDictionary = new();

		var leftPairs = leftDistribution.ToDictionary().ToArray();

		Parallel.ForEach(leftPairs, leftPair =>
		{
			Dictionary<double, double> localDict = new();
			double leftValue = leftPair.Key;
			double leftProb = leftPair.Value;
			foreach (var rightPair in rightDistribution.ToDictionary())
			{
				double rightValue = rightPair.Key;
				double rightProb = rightPair.Value;
				var diceDistribution = new UniformRangeDice(1, (long)rightValue, (int)leftValue).ToDictionary();
				foreach (var dicePair in diceDistribution.ToDictionary())
				{
					double diceValue = dicePair.Key;
					double diceProb = dicePair.Value;
					double combinedProb = leftProb * rightProb * diceProb;
					if (localDict.ContainsKey(diceValue))
					{
						localDict[diceValue] += combinedProb;
					}
					else
					{
						localDict[diceValue] = combinedProb;
					}
				}
			}
			foreach (var kvp in localDict)
			{
				concurrentDistributionDictionary.AddOrUpdate(kvp.Key, kvp.Value, (key, oldValue) => oldValue + kvp.Value);
			}
		});

		return ProbabilisticNumber<double>.FromDictionary(concurrentDistributionDictionary.ToDictionary());
	}
}

public class NegationNode : UnaryOperatorNode
{
    public override double Evaluate() => -Child!.Evaluate();
	public override ProbabilisticNumber<double> GetDistribution()
    {
		var distribution = Child!.GetDistribution();
		distribution.ModifyValues(value => -value);
		return distribution;
	}
}
namespace DiceEquationCLI;
public interface IOperatorNode { }

public abstract class BinaryOperatorNode : BinaryNode, IOperatorNode { }
public abstract class UnaryOperatorNode : UnaryNode, IOperatorNode { }

public class AdditionNode : BinaryOperatorNode
{
    public override double Evaluate() => Left.Evaluate() + Right.Evaluate();
}

public class SubtractionNode : BinaryOperatorNode
{
    public override double Evaluate() => Left.Evaluate() - Right.Evaluate();
}

public class MultiplicationNode : BinaryOperatorNode
{
    public override double Evaluate() => Left.Evaluate() * Right.Evaluate();
}

public class DivisionNode : BinaryOperatorNode
{
    public override double Evaluate() {
        if (Right.Evaluate() == 0)
        {
            throw new DivideByZeroException("Division by zero is not allowed.");
        }
        return Left.Evaluate() / Right.Evaluate();
    }
}

public class DiceSimulationNode : BinaryOperatorNode
{
    // Note: because the left and right values aren't necessarily deterministic,
    // we can't create a single UniformRangeDice and use that for Evaluate.

    Random rng= new Random();

	public override double Evaluate()
    {
        double leftValue = Left.Evaluate();
        double rightValue = Right.Evaluate();

        if (leftValue < 0 || Math.Round(leftValue) != leftValue)
		{
			throw new ArgumentException("Invalid dice roll count: Must be a non-negative integer.");
		}

        if (rightValue < 0 || Math.Round(rightValue) != rightValue)
		{
			throw new ArgumentException("Invalid dice sides: Must be a positive integer.");
		}
        if (rightValue >= Int64.MaxValue) throw new ArgumentException($"Invalid dice sides: dice cannot have more than {Int64.MaxValue - 1} sides");
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
}

public class NegationNode : UnaryOperatorNode
{
    public override double Evaluate() => -Child.Evaluate();
}
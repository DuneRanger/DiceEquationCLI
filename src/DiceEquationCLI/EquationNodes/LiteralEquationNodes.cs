using ProbabilisticNumberLibrary;

namespace DiceEquationCLI;
public interface INumberLiteralNode { }

public class IntegerNode(long Value) : ValueNode, INumberLiteralNode
{
    public override double Evaluate() => Value;
	public override ProbabilisticNumber<double> GetDistribution() => ProbabilisticNumber<double>.FromUniform([Value]);
}

public class DoubleNode(double Value) : ValueNode, INumberLiteralNode
{
    public override double Evaluate() => Value;
	public override ProbabilisticNumber<double> GetDistribution() => ProbabilisticNumber<double>.FromUniform([Value]);
}

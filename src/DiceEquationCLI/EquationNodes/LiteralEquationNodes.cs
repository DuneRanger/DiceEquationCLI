namespace DiceEquationCLI;
public interface INumberLiteral { }

public class Integer(long Value) : ValueNode, INumberLiteral
{
    public override double Evaluate() => Value;
}

public class Double(double Value) : ValueNode, INumberLiteral
{
    public override double Evaluate() => Value;
}

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
    public override double Evaluate() => Left.Evaluate() / Right.Evaluate();
}

public class DiceConstructionNode : BinaryOperatorNode
{

    public override double Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class NegationNode : UnaryOperatorNode
{
	public override double Evaluate() => -Child.Evaluate();
}
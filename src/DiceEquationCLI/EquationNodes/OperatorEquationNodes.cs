namespace DiceEquationCLI;
public interface IOperator { }

public abstract class BinaryOperator : BinaryNode, IOperator
{
    public BinaryOperator(Node left, Node right) : base(left, right) { }
}

public class Addition : BinaryOperator
{
    public Addition(Node left, Node right) : base(left, right) { }
    public override double Evaluate() => Left.Evaluate() + Right.Evaluate();
}

public class Subtraction : BinaryOperator
{
    public Subtraction(Node left, Node right) : base(left, right) { }
    public override double Evaluate() => Left.Evaluate() - Right.Evaluate();
}

public class Multiplication : BinaryOperator
{
    public Multiplication(Node left, Node right) : base(left, right) { }
    public override double Evaluate() => Left.Evaluate() * Right.Evaluate();
}

public class Division : BinaryOperator
{
    public Division(Node left, Node right) : base(left, right) { }
    public override double Evaluate() => Left.Evaluate() / Right.Evaluate();
}

public class DiceConstruction : BinaryOperator
{
    public DiceConstruction(Node left, Node right) : base(left, right) { }

    public override double Evaluate()
    {
        throw new NotImplementedException();
    }
}

namespace DiceEquationCLI;

public abstract class Node
{
    public abstract double Evaluate();
}

public abstract class ValueNode : Node { }

public abstract class UnaryNode : Node
{
    public Node Child { get; }
    public UnaryNode(Node child)
    {
        Child = child;
    }
}

public abstract class BinaryNode : Node
{
    public Node Left { get; }
    public Node Right { get; }
    public BinaryNode(Node left, Node right)
    {
        Left = left;
        Right = right;
    }
}
namespace DiceEquationCLI;

public abstract class Node
{
    public abstract double Evaluate();
}

public abstract class ValueNode : Node { }

public abstract class UnaryNode : Node
{
    public Node? Child { get; private set; }

    public void AddChild(Node child)
    {
        if (Child is not null) throw new InvalidOperationException("Unary node already has a child");
        Child = child;
    }
}

public abstract class BinaryNode : Node
{
    public Node? Left { get; private set; }
    public Node? Right { get; private set; }
    
    public void AddLeftChild(Node child)
    {
        if (Left is not null) throw new InvalidOperationException("Binary node already has a left child");
        Left = child;
    }
    
    public void AddRightChild(Node child)
    {
        if (Right is not null) throw new InvalidOperationException("Binary node already has a right child");
        Right = child;
    }

    public void AddChildren(Node left, Node right)
    {
        AddLeftChild(left);
        AddRightChild(right);
    }
}
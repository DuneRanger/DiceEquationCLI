using ProbabilisticNumberLibrary;

namespace DiceEquationCLI;

public class Equation
{
    public string Name { get; private set; } = "";
    private string _equation = "";
    private Node? _equationTree;
    private IDiscreteDistribution<double>? _distribution;

    public Equation(string equation)
    {
        _equation = equation;
    }
    
    public void SetName(string name) => Name = name;
    public void SetEquationTree(Node equationTreeRoot) => _equationTree = equationTreeRoot;
    public void SetDistribution(IDiscreteDistribution<double> distribution) => _distribution = distribution;
    
    public double Evaluate() {
        if (_distribution is not null) return _distribution.Sample();
        if (_equationTree is not null) return _equationTree.Evaluate();
        throw new InvalidOperationException("No distribution available");
    }
    public IDiscreteDistribution<double> GetDistribution() => _distribution ?? throw new InvalidOperationException("No distribution available");
    public override string ToString() => _equation;
}
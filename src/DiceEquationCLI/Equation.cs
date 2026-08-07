using ProbabilisticNumberLibrary;

namespace DiceEquationCLI;

public class Equation
{
    public string Name { get; private set; }
    private string _equation;
    private Node _equationTree;
    private IDiscreteDistribution<double> _distribution;

    public Equation(string equation)
    {
        _equation = equation;
    }
    
    public void SetName(string name) => Name = name;
    public void SetEquationTree(Node equationTreeRoot) => _equationTree = equationTreeRoot;
    public void SetDistribution(IDiscreteDistribution<double> distribution) => _distribution = distribution;
    
    public double Evaluate() => _distribution.Sample();
    public IDiscreteDistribution<double> GetDistribution() => _distribution;
    public override string ToString() => _equation;
}
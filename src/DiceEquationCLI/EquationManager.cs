namespace DiceEquationCLI;

public class EquationManager
{
    private Dictionary<string, Equation> _equations = new();
    
    public void AddEquation(string name, string equation)
    {
        var equationObj = EquationParser.Parse(equation);
        equationObj.SetName(name);
        _equations.Add(name, equationObj);
    }

    public bool HasEquation(string name)
    {
        return _equations.ContainsKey(name);
    }
    
    public Equation GetEquation(string name)
    {
        _equations.TryGetValue(name, out Equation? equation);
        if (equation == null) throw new ArgumentException("No such equation: " + name);
        return equation;
    }
    
    public void DeleteEquation(string name)
    {
        if (!_equations.ContainsKey(name)) throw new ArgumentException("No such equation: " + name);
        _equations.Remove(name);
    }

    public IEnumerable<KeyValuePair<string, Equation>> EnumerateEquations()
    {
        return _equations.AsEnumerable();
    }

    public void ClearEquations()
    {
        _equations.Clear();
    }
}
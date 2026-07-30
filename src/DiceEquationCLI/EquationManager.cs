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
    
    public Equation GetEquation(string name)
    {
        return _equations[name];
    }
    
    public void DeleteEquation(string name)
    {
        _equations.Remove(name);
    }
}
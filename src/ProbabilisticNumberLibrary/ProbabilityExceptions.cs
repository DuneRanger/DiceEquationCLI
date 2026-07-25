using System.Numerics;
using System.Reflection.Metadata;

namespace ProbabilisticNumberLibrary;

public class ProbabilityException : ApplicationException
{
    public ProbabilityException(string message) : base(message) { }
}

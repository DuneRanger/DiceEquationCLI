using ProbabilisticNumberLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTests
{
	public static class DistributionTestHelper
	{
		public static bool AreEqual<T>(IDiscreteDistribution<T> expected, IDiscreteDistribution<T> actual, double tolerance = 1e-10)
			where T : notnull
		{
			var expectedDict = expected.ToDictionary();
			var actualDict = actual.ToDictionary();

			return AreDictionariesEqual(expectedDict, actualDict, tolerance);
		}

		public static bool AreDictionariesEqual<T>(Dictionary<T, double> expected, Dictionary<T, double> actual, double tolerance = 1e-10)
			where T : notnull
		{
			if (expected.Count != actual.Count)
				return false;

			foreach (var kvp in expected)
			{
				if (!actual.TryGetValue(kvp.Key, out double actualProb))
					return false;

				if (Math.Abs(kvp.Value - actualProb) > tolerance)
					return false;
			}

			return true;
		}
	}
}

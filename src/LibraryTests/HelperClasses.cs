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
		/// <summary>
		/// Compares two discrete distributions for equality within a specified tolerance.
		/// Specifically, the equality of their dictionaries (without error) is checked.
		/// </summary>
		/// <typeparam name="T">The type of the values in the distributions.</typeparam>
		/// <param name="expected">The expected distribution.</param>
		/// <param name="actual">The actual distribution.</param>
		/// <param name="tolerance">The tolerance for the comparison of probabilties.</param>
		/// <returns>True if the distributions are equal within the specified tolerance, otherwise, false.</returns>
		public static bool AreEqual<T>(IDiscreteDistribution<T> expected, IDiscreteDistribution<T> actual, double tolerance = 1e-10)
			where T : notnull
		{
			var expectedDict = expected.ToDictionary();
			var actualDict = actual.ToDictionary();

			return AreDictionariesEqual(expectedDict, actualDict, tolerance);
		}

		/// <summary>
		/// Compares two dictionaries for equality within a specified tolerance.
		/// The comparison is based on the keys and their corresponding double values, with the specified tolerance for the double values.
		/// </summary>
		/// <typeparam name="T">The type of the keys in the dictionaries.</typeparam>
		/// <param name="expected">The expected dictionary.</param>
		/// <param name="actual">The actual dictionary.</param>
		/// <param name="tolerance">The tolerance for the comparison of double values.</param>
		/// <returns>True if the dictionaries are equal within the specified tolerance, otherwise, false.</returns>
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

	public class HelperClassTests
	{
		#region ProbabilisticNumber<T> DistributionTestHelper Equality Tests

		[Fact]
		public void DistributionEquality_SameDistributions()
		{
			var prob1 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var prob2 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });

			Assert.True(DistributionTestHelper.AreEqual(prob1, prob2));
		}

		[Fact]
		public void DistributionEquality_DifferentDistributions()
		{
			var prob1 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var prob2 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.3 }, { 2, 0.7 } });

			Assert.False(DistributionTestHelper.AreEqual(prob1, prob2));
		}

		[Fact]
		public void DistributionEquality_WithinTolerance()
		{
			var prob1 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var prob2 = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.50000001 }, { 2, 0.49999999 } });

			Assert.True(DistributionTestHelper.AreEqual(prob1, prob2, 1e-7));
		}

		#endregion ProbabilisticNumber<T> DistributionTestHelper Equality Tests
	}
}

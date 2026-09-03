using DiceNumberLibrary;
using ProbabilisticNumberLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTests
{
	public class DiceNumberTests
	{
		// ========== Helper Assert Functions ==========
		private void AssertDistributionEquals<T>(IDiscreteDistribution<T> expected, IDiscreteDistribution<T> actual, double tolerance = 1e-10)
			where T : notnull
		{
			Assert.True(DistributionTestHelper.AreEqual(expected, actual, tolerance),
				"Distributions are not equal");
		}
		private void AssertDictionaryEquals<T>(Dictionary<T, double> expected, Dictionary<T, double> actual, double tolerance = 1e-10)
			where T : notnull
		{
			Assert.True(DistributionTestHelper.AreDictionariesEqual(expected, actual, tolerance),
				"Dictionaries are not equal");
		}

		// ========== Basic DistributionDice Tests ==========

		[Fact]
		public void DistributionDice_ZeroRolls_ReturnsZero()
		{
			var baseDist = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var dice = new DistributionDice<int>(baseDist, 0);

			var result = dice.ToDiscreteDistribution();
			var dict = result.ToDictionary();

			Assert.Single(dict);
			Assert.Equal(1.0, dict[0]);
		}

		[Fact]
		public void DistributionDice_OneRoll_ReturnsSingleDistribution()
		{
			var baseDist = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var dice = new DistributionDice<int>(baseDist, 1);

			var result = dice.ToDiscreteDistribution();
			var expected = baseDist;

			AssertDistributionEquals(expected, result);
		}

		[Fact]
		public void DistributionDice_MultipleRolls_CombinesDistributions()
		{
			var baseDist = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var dice = new DistributionDice<int>(baseDist, 3);

			var result = dice.ToDiscreteDistribution();
			var dict = result.ToDictionary();

			var expectedDict = new Dictionary<int, double>
			{
				{ 3, 0.125 },
				{ 4, 0.375 },
				{ 5, 0.375 },
				{ 6, 0.125 }
			};

			AssertDictionaryEquals(expectedDict, dict);
		}

		[Fact]
		public void DistributionDice_NegativeRollCount_ThrowsException()
		{
			var baseDist = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 1.0 } });

			Assert.Throws<ArgumentException>(() => new DistributionDice<int>(baseDist, -1));
		}

		// ========== Basic UniformRangeDice Tests ==========

		[Fact]
		public void UniformRangeDice_ZeroRolls_ReturnsZero()
		{
			var dice = new UniformRangeDice(1, 6, 0);
			var result = dice.ToDiscreteDistribution();
			var dict = result.ToDictionary();

			Assert.Single(dict);
			Assert.Equal(1.0, dict[0]);
		}

		[Fact]
		public void UniformRangeDice_OneRoll_ReturnsUniformDistribution()
		{
			var dice = new UniformRangeDice(1, 6, 1);
			var result = dice.ToDiscreteDistribution();
			var dict = result.ToDictionary();

			Assert.Equal(5, dict.Count);
			foreach (var v in dict.Values)
			{
				Assert.Equal(1.0 / 5, v, 10);
			}
		}

		[Fact]
		public void UniformRangeDice_MinGreaterThanMax_ThrowsException()
		{
			Assert.Throws<ArgumentException>(() => new UniformRangeDice(6, 1, 1));
		}

		[Fact]
		public void UniformRangeDice_NegativeRollCount_ThrowsException()
		{
			Assert.Throws<ArgumentException>(() => new UniformRangeDice(1, 6, -1));
		}
	}
}

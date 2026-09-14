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
		#region Helper Assert Functions
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

		#endregion Helper Assert Functions
		#region Basic DistributionDice Tests

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

		#endregion Basic DistributionDice Tests
		#region Basic UniformRangeDice Tests

		[Fact]
		public void UniformRangeDice_ZeroRolls_ReturnsZero()
		{
			var dice = new UniformRangeDice(1, 6, 0);
			var dict = dice.ToDictionary();

			Assert.Single(dict);
			Assert.Equal(1.0, dict[0]);
		}

		[Fact]
		public void UniformRangeDice_OneRoll_ReturnsUniformDistribution()
		{
			var dice = new UniformRangeDice(1, 6, 1);
			var dict = dice.ToDictionary();

			Assert.Equal(6, dict.Count);
			foreach (var v in dict.Values)
			{
				Assert.Equal(1.0 / 6, v, 10);
			}
		}

		[Fact]
		public void UniformRangeDice_MultipleRolls_CorrectlyAddsRolls()
		{
			var dice = new UniformRangeDice(1, 6, 2);
			var dict = dice.ToDictionary();

			Assert.Equal(11, dict.Count);
			Assert.Equal(1.0/36, dict[2], 10);
			Assert.Equal(2.0/36, dict[3], 10);
			Assert.Equal(3.0/36, dict[4], 10);
			Assert.Equal(4.0/36, dict[5], 10);
			Assert.Equal(5.0/36, dict[6], 10);
			Assert.Equal(6.0/36, dict[7], 10);
			Assert.Equal(5.0/36, dict[8], 10);
			Assert.Equal(4.0/36, dict[9], 10);
			Assert.Equal(3.0/36, dict[10], 10);
			Assert.Equal(2.0/36, dict[11], 10);
			Assert.Equal(1.0/36, dict[12], 10);
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
		
		#endregion Basic UniformRangeDice Tests
	}
}

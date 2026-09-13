using ProbabilisticNumberLibrary;
using DiceNumberLibrary;
using System.Numerics;

namespace LibraryTests
{
	public class ProbabilisticNumberTests
	{
		#region ProbabilisticNumber<T> Construction Tests

		[Fact]
		public void ValidConstruction_CreatesInstance()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 0.2, 0.3, 0.5 };
			var prob = new ProbabilisticNumber<int>(values, probs);

			Assert.Equal(3, prob.Count);
		}

		[Fact]
		public void BadInputsConstruction_ThrowsException()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 0.2, 0.3 };

			Assert.Throws<ArgumentException>(() => new ProbabilisticNumber<int>(values, probs));
		}

		[Fact]
		public void FromDictionary_EquivalentInstance()
		{
			var dict = new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } };
			var probFromDict = ProbabilisticNumber<int>.FromDictionary(dict);
			var probDirect = new ProbabilisticNumber<int>(new int[] { 1, 2 }, new double[] { 0.5, 0.5 });

			Assert.Equal(probFromDict.ToDictionary(), probDirect.ToDictionary());
		}

		[Fact]
		public void FromUniform_CreatesUniformDistribution()
		{
			var values = new int[] { 1, 2, 3, 4 };
			var prob = ProbabilisticNumber<int>.FromUniform(values);

			var dict = prob.ToDictionary();
			Assert.Equal(4, dict.Count);
			foreach (var kvp in dict)
			{
				Assert.Equal(0.25, kvp.Value, 10);
			}
		}

		[Fact]
		public void FromUniformRangeInt_CreatesUniformRangeDistribution()
		{
			var rangeMin = 1;
			var rangeMax = 4;
			var prob = ProbabilisticNumber<int>.FromUniformRange(rangeMin, rangeMax);

			var dict = prob.ToDictionary();
			Assert.Equal(3, dict.Count);
			for (var i = rangeMin; i < rangeMax; i++)
			{
				Assert.True(dict.ContainsKey(i));
				Assert.Equal(1.0 / (rangeMax - rangeMin), dict[i], 10);
			}
		}

		[Fact]
		public void FromUniformRangeLong_CreatesUniformRangeDistribution()
		{
			var rangeMin = 1000000000001;
			var rangeMax = 1000000000004;
			var prob = ProbabilisticNumber<long>.FromUniformRange(rangeMin, rangeMax);

			var dict = prob.ToDictionary();
			Assert.Equal(3, dict.Count);
			for (var i = rangeMin; i < rangeMax; i++)
			{
				Assert.True(dict.ContainsKey(i));
				Assert.Equal(1.0 / (rangeMax - rangeMin), dict[i], 10);
			}
		}

		#endregion ProbabilisticNumber<T> Construction Tests
		#region ProbabilisticNumber<T> Probability/Value Modification Tests

		[Fact]
		public void Normalize_AdjustsProbabilities()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 1, 2, 3 };
			var sumProbs = probs.Sum();

			var prob = new ProbabilisticNumber<int>(values, probs);

			prob.Normalize();
			var dict = prob.ToDictionary();

			double sum = dict.Values.Sum();
			Assert.Equal(1.0, sum, 10);
			Assert.Equal(1.0 / sumProbs, dict[1], 10);
			Assert.Equal(2.0 / sumProbs, dict[2], 10);
			Assert.Equal(3.0 / sumProbs, dict[3], 10);
		}

		[Fact]
		public void PurgeValues_RemovesMatchingValues()
		{
			var values = new int[] { 1, 2, 3, 4, 5 };
			var probs = new double[] { 0.2, 0.2, 0.2, 0.2, 0.2 };
			var prob = new ProbabilisticNumber<int>(values, probs);

			prob.PurgeValues(v => v > 3);

			var dict = prob.ToDictionary();
			Assert.Equal(3, dict.Count);
			Assert.True(dict.ContainsKey(1));
			Assert.True(dict.ContainsKey(2));
			Assert.True(dict.ContainsKey(3));
			Assert.False(dict.ContainsKey(4));
			Assert.False(dict.ContainsKey(5));
		}

		[Fact]
		public void PurgeValues_RemovesAllValues()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 0.3, 0.3, 0.4 };
			var prob = new ProbabilisticNumber<int>(values, probs);
			prob.PurgeValues(v => v > 0);
			var dict = prob.ToDictionary();
			Assert.Empty(dict);
		}

		[Fact]
		public void NormalizeAfterPurge_AdjustsProbabilities()
		{
			var values = new int[] { 1, 2, 3, 4, 5 };
			var probs = new double[] { 0.2, 0.2, 0.2, 0.2, 0.2 };
			var prob = new ProbabilisticNumber<int>(values, probs);
			prob.PurgeValues(v => v > 3);
			prob.Normalize();
			var dict = prob.ToDictionary();
			Assert.Equal(3, dict.Count);
			Assert.Equal(1.0 / 3, dict[1], 10);
			Assert.Equal(1.0 / 3, dict[2], 10);
			Assert.Equal(1.0 / 3, dict[3], 10);
		}

		[Fact]
		public void ModifyValues_TransformsAllValues()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 0.3, 0.3, 0.4 };
			var prob = new ProbabilisticNumber<int>(values, probs);

			prob.ModifyValues(v => v * 2);

			var dict = prob.ToDictionary();
			Assert.True(dict.ContainsKey(2));
			Assert.True(dict.ContainsKey(4));
			Assert.True(dict.ContainsKey(6));

			Assert.False(dict.ContainsKey(1));
			Assert.False(dict.ContainsKey(3));
		}

		[Fact]
		public void ModifyValues_TransformsValuesToSameValue()
		{
			var values = new int[] { 1, 2, 3 };
			var probs = new double[] { 0.3, 0.3, 0.4 };
			var prob = new ProbabilisticNumber<int>(values, probs);
			prob.ModifyValues(v => 5);
			var dict = prob.ToDictionary();
			Assert.Single(dict);
			Assert.True(dict.ContainsKey(5));
			Assert.Equal(1.0, dict[5], 10);
		}

		#endregion ProbabilisticNumber<T> Probability/Value Modification Tests
		#region ProbabilisticNumber<T> ToDictionary Tests

		[Fact]
		public void ToDictionary_InverseToFromDictionary()
		{
			var dict = new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } };
			var prob = ProbabilisticNumber<int>.FromDictionary(dict);

			var resultDict = prob.ToDictionary();
			Assert.Equal(dict, resultDict);
		}

		[Fact]
		public void ToDictionaryWithError_ContainsError()
		{
			var prob = ProbabilisticNumber<int>.FromUniformRange(1, 6);
			prob.PurgeValues(v => v > 3);
			var resultDict = prob.ToDictionaryWithError();

			var expectedDict = new Dictionary<string, double>
			{
				{ "1", 1.0 / 5 },
				{ "2", 1.0 / 5 },
				{ "3", 1.0 / 5 },
				{ "error", 2.0 / 5 }
			};

			DistributionTestHelper.AreDictionariesEqual(expectedDict, resultDict, 1e-10);
		}

		[Fact]
		public void ToDictionaryWithError_ContainsAccumulatedError()
		{
			var prob = ProbabilisticNumber<int>.FromUniformRange(1, 6);
			prob.PurgeValues(v => v > 3);
			prob.PurgeValues(v => v == 3);
			var resultDict = prob.ToDictionaryWithError();

			var expectedDict = new Dictionary<string, double>
			{
				{ "1", 1.0 / 5 },
				{ "2", 1.0 / 5 },
				{ "error", 3.0 / 5 }
			};

			DistributionTestHelper.AreDictionariesEqual(expectedDict, resultDict, 1e-10);
		}

		#endregion ProbabilisticNumber<T> ToDictionary Tests
		#region ProbabilisticNumber<T> Convolution/Operator Tests

		[Fact]
		public void AdditionOperator_WithSingleValues()
		{
			var left = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 5, 1.0 } });
			var right = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 3, 1.0 } });

			var result = left + right;
			var dict = result.ToDictionary();

			Assert.Single(dict);
			Assert.Equal(1.0, dict[8]);
		}

		[Fact]
		public void AdditionOperator_CombinesTwoDistributions()
		{
			var left = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });
			var right = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 1, 0.5 }, { 2, 0.5 } });

			var result = left + right;
			var dict = result.ToDictionary();

			// 1+1=2, 1+2=3, 2+1=3, 2+2=4
			Assert.Equal(3, dict.Count);
			Assert.Equal(0.25, dict[2], 10);
			Assert.Equal(0.5, dict[3], 10);
			Assert.Equal(0.25, dict[4], 10);
		}

		[Fact]
		public void SubtractionOperator_CombinesTwoDistributions()
		{
			var left = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 5, 0.5 }, { 10, 0.5 } });
			var right = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 2, 0.5 }, { 3, 0.5 } });

			var result = left - right;
			var dict = result.ToDictionary();

			// 5-2=3, 5-3=2, 10-2=8, 10-3=7
			Assert.Equal(4, dict.Count);
			Assert.Equal(0.25, dict[3], 10);
			Assert.Equal(0.25, dict[2], 10);
			Assert.Equal(0.25, dict[8], 10);
			Assert.Equal(0.25, dict[7], 10);
		}

		[Fact]
		public void MultiplicationOperator_CombinesTwoDistributions()
		{
			var left = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 2, 0.5 }, { 3, 0.5 } });
			var right = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 2, 0.5 }, { 3, 0.5 } });

			var result = left * right;
			var dict = result.ToDictionary();

			// 2*2=4, 2*3=6, 3*2=6, 3*3=9
			Assert.Equal(3, dict.Count);
			Assert.Equal(0.25, dict[4], 10);
			Assert.Equal(0.5, dict[6], 10);
			Assert.Equal(0.25, dict[9], 10);
		}

		[Fact]
		public void DivisionOperator_CombinesTwoDistributions_IntValues()
		{
			var left = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 6, 0.5 }, { 9, 0.5 } });
			var right = ProbabilisticNumber<int>.FromDictionary(new Dictionary<int, double> { { 2, 0.5 }, { 3, 0.5 } });

			var result = left / right;
			var dict = result.ToDictionary();

			// 6/2=3, 6/3=2, 9/2=4, 9/3=3
			Assert.Equal(3, dict.Count);
			Assert.Equal(0.5, dict[3], 10);
			Assert.Equal(0.25, dict[2], 10);
			Assert.Equal(0.25, dict[4], 10);
		}

		[Fact]
		public void DivisionOperator_CombinesTwoDistributions_DoubleValues()
		{
			var left = ProbabilisticNumber<double>.FromDictionary(new Dictionary<double, double> { { 6.0, 0.5 }, { 9.0, 0.5 } });
			var right = ProbabilisticNumber<double>.FromDictionary(new Dictionary<double, double> { { 2.0, 0.5 }, { 3.0, 0.5 } });

			var result = left / right;
			var dict = result.ToDictionary();

			// 6/2=3, 6/3=2, 9/2=4.5, 9/3=3
			Assert.Equal(3, dict.Count);
			Assert.Equal(0.5, dict[3], 10);
			Assert.Equal(0.25, dict[2], 10);
			Assert.Equal(0.25, dict[4.5], 10);
		}

		#endregion ProbabilisticNumber<T> Convolution/Operator Tests
	}
}


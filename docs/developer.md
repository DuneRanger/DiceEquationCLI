# Developer Documentation

## Introduction

## Probabilistic Number Library

The probabilistic number library is a small library that provides classes and interfaces for working with probability distributions.
Currently, it only contains a single class, aimed at representing a discrete probability distribution of numbers.

### ProbabilisticNumber<T>

Represents a discrete probability distribution of numbers.

#### Internal representation

The values and probabilities are seperated into two arrays. It is assumed that the array of values and probabilities are of the same length and that each index in the arrays corresponds to a value and probability pair.
The burden of maintaining the matching of indices between the arrays is left to the developer of any function modifying them.
Of course, the ordering itself does not need to be maintained, only the matching indices.

There are no safeguards that check that the sum of probabilities is truly 1. Constructing a distribution with a sum of probabilities less than or larger than 1 is valid behaviour.
No functions should assume that the sum of probabilities is 1, apart from functions sampling from the distribution.

#### Sampling

Sampling a values simply goes through the array of probabilities and subtracts them from a random number between 0 and 1, until the sum of the probabilities is more than or equal to the random number.
If the generated number is larger than the sum of the probabilities, an error is thrown instead.
This could occur in two cases:
- The random number infinitessimaly close to 1 and precision problems occur when subtracting the probabilities.
- The sum of probabilities itself is less than 1.

The first situation is hard to avoid and I believe the odds of it happening are small enough that throwing an error an acceptable resolution.
The second situation is intended, so that distributions can return something other than a value of the type they store.

#### Modifying the distribution

The distribution implements basic operators to allow for easy combination of distributions, as if applying an operator to two random variables.
There are no operators for modifying the values of the distribution, so there isn't any potential confusion whether an expression like `distribution + 5` adds 5 to every value, or adds 5 to the distribution.

Directly modifying the values is allowed through a `ModifyValues(Func<T, T>)` method, and any modification of internal fields should be done with similarly, disallowing direct access to the arrays.

Though a bit a strange decision, removing values from the distribution and recalculating the weights so that their sum is 1, is seperated into two different functions.
This is done to allow for removing unwanted values from the distribution and "replacing" them with an error instead, for example if I only want to accept integer values from a distribution containing both integers and rationals.

The rationale behind this decision is that despite it being possible to do on the user-side, applying such a filter and then combining with another distribution would otherwise require creating a new filtered distribution, which is just unnecessary additional work for the user.
Instead it is supported by the library and the normalization of the probabilities must be called explicitly.

#### Serialization and deserialization

The first thing to note is that there may be duplicate values in the distribution.
Serialization and deserialization methods are allowed to aggregate the probabilities of duplicate values into a single probability.

## Dice Number Library


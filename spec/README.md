# Specification of the final project for relevant C# courses
(When modifying this document, please maintain the layout and structure and follow the inline instructions.)

## C# Courses selection
(Change `[ ]` to `[x]` for the courses you plan to use this final project for.)

- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [x] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [ ] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [ ] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### Probabilistic number library and equation calculator CLI

This project will provide a library for working with probabilistic/non-deterministic numbers, a library for working with "dice" numbers, built upon the first library, and finally, a CLI interface for an equation solver program.

#### Probabilistic number library

This library will provide a set of classes for working with probabilistic numbers.
It will provide a number of constructors for converting deterministic numbers, or lists of probabilities, to probabilistic numbers.
Methods for calculating the sum/difference/product/quotient of probabilistic numbers will be provided.

Because it is expected that the possible outcomes of a probabilistic number will be large, the library may use parallelism during the calculation of more complex operations (i.e. multiplication and division).
The library will also contain methods for serializing/deserializing probabilistic numbers.

The probabilities will be represented as a double value between 0 and 1.
The values of a probabilistic number can be any type implemented the `INumber` interface.

Probabilistic numbers may also be evaluated/collapsed, where one of the outcomes is chosen at random.

#### Dice number library

This library will provide a small set of classes for working with "dice" numbers.
"Dice" numbers are a pair of numbers, the first representing the number of dice, the second the maximum value of the dice.
In the expression `2d6`, 2 will be called the *count* and 6 the *distribution* and the expression represents a probabilistic number equal to the sum of two fair dice rolls.

One way to look at it is that the distribution defines a discrete probability distribution, and the count defines how many times the distribution is repeated and summed.
Thus, when writing a simple/primitive dice number, the value must be an integer, so that it can be transformed into a probabilistic number with uniform probability.
- The expression "1d4" is legal and simulates the roll of a fair 4-sided dice.
- The expression "4d0" is legal and simulates four rolls of a "fair 0-sided dice" (thus the result is always 0).
- The expression "2d3.5" is illegal, as there is no clear way to choose values for the dice.
- The expression "2d-3" is illegal, as there is no clear way to choose values for the dice.

In terms of a more complex expression, "2d(3d2 / 2)", although the value of "(3d2 / 2)" may not be an integer, it itself is a discrete probability distribution (AKA a _probabilistic number_ from the aforementioned library). As such, the expression is calculable and valid.

A more complex count expression is also possible, for example, "(3d2 / 2)d6" and will be probabilistically calculated, however, noninteger count values will be considered an error.

The exact mechanism of this may be subject to change (since I'm not sure how well it'll work). However, the idea is that all noninteger values will be removed and a probability of an "error" will be introduced, it being the summed probabilities of removed noninteger values.
Then, when using the expression in further calculations, the error probability will be propagated further. When serializing the expression, a special "error" field will appear, and when attempting to evaluate the expression, an error may be displayed (together with its probability).


##### Syntax summary

A dice number is an expression in the form of `XdY`, where both `X` and `Y` are either non-negative integers or a probabilistic number.
When `X` is a probabilistic number, the probability of any noninteger values is aggregated into a single error probability.
Then, when the expression is evaluated, there exists a probability of getting an error, instead of an actual value.

#### Equation solver CLI

The equation solver will be a command-line interface (CLI) application, supporting the following features:
 - Parsing of expressions
 - Saving expressions under a name
 - Evaluation of expressions
 - Displaying parsed expressions
 - Displaying the probability distribution of an expression
 - Serializing/deserializing expressions
 - Loading expressions from a file
 - Saving expressions to a file
 - Displaying the help/usage information

First, I'll show an example of a session's commands, then I'll go through and describe each of their capabilities.
```shell
> help
[help message]
> save a = 2d6 + 3
saved 'a'
> eval a
11
> eval a
5
> show a
2d6 + 3
> export only_a.txt
exported 'a' to 'only_a.txt'
> import only_a.txt
Error: expression 'a' already exists
> del a
deleted 'a'
> import only_a.txt
imported 'a'
> save b = 10d2 * 5d(5 + 2d2)
saved 'b'
> export all.txt
exported 'a', 'b' to 'all.txt'
> del all
> import all.txt
imported 'a', 'b' from 'all.txt'
> show all
a = 2d6 + 3
b = 10d2 * 5d(5 + 2d2)
> eval all
a = 9
b = 105
> save a = (1d4 / 2)d(1d4 / 2)
saved 'a'
> prob a
Error: 50%
0.5: 6.25%
1: 12.5%
1.5: 6.25%
2: 12.5%
3: 6.25%
4: 6.25%
```

Note: `a = (1d4 / 2)d(1d4 / 2)` has a 50% chance to fail, due to the first `(1d4 / 2)` not evaluating to an integer.
Then the remaining options are (1|2)d(0.5|1|1.5|2), where I am using `|` to denote uniform probability between options.
Summing all the possible values, you get the listed probabilities from the example

- `save` and `del` commands will be used to save and delete expressions.
- `eval` command will be used to evaluate expressions.
- `show` command will be used to display the parsed expression.
- `export` command will export all saved expressions to a specified file.
- `import` command will be used to import as many expressions as possible from a file.
  - If an already existing or invalid expression is imported, an error will be printed, but the import will still continue.
- `help` command will be used to display the help/usage information.
- `prob` will display the probability distribution of an expression

Commands `del`, `eval` and `show` will support the keyword `all` that will apply the command to all saved expressions.
Despite `prob` being a command that could support `all`, I decided it shouldn't, because I expect the output to be too long for practical use.

##### Expression syntax

The CLI application will support the following expression syntax:
- binary operators: `+`, `-`, `*`, `/`, `d` (dice)
- unary operators: `-`
- only integers and dice numbers will be allowed as operands in expressions
  - so `b = a + 2d6` is invalid syntax
- parentheses are allowed and must contain a complete expression
- whitespace will be ignored
- no strings or identifiers are allowed

Operator precedence (in order):
- parentheses
- dice rolls
- unary minus
- multiplication and division
- addition and subtraction

So `-2d6 + 3d(4 + 3)` will be parsed as `(-(2d6)) + (3d(4 + 3))`

##### Exporting and importing

Exporting expressions will write them all in string representation: `a = 2d6 + 3` with each expression on a new line.
Importing expressions will read them all from a file, importing all expressions that can be parsed and do not already exist.

##### Structure

The CLI application will be divided into two parts:
- the internal infrastructure with the core functionality of the application
  - will include an expression parser, storage, evaluation and exporting/importing
- the CLI application that will work with the internal infrastructure
  - Simply parses the command line arguments and calls functions from the internal infrastructure with the corresponding arguments.

## Motivation

Part of this application can be used for deciding the outcome of multiple dice rolls in complex situations for some RPG games, though that most likely only makes use of the addition of simple dice rolls.
The extension of that functionality (division and multiplication) is more of a fun project than a practically useful tool, due to the difficulty of balancing games with such equations, however, I imagine the core developed libraries (probabilistic numbers and dice numbers) could be reused within a game and the CLI application could be considered a proof of concept for observing how different complex dice equations can behave.

## Testing

The probabilistic number library and the dice number library will be tested to verify that the library works as expected and the arithmetic operations are correct to a certain degree of precision.
The equation solver CLI application has its internal infrastructure tested with unit tests, whereas the CLI interface will be tested manually.

All automated tests will be done with XUnit.
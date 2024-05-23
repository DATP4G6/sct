![.NET status](https://github.com/DATP4G6/sct/actions/workflows/dotnet.yml/badge.svg)
![Format status](https://github.com/DATP4G6/sct/actions/workflows/dotnet-format.yml/badge.svg)

# Societal Construction Tool

This repository contains the source code for the *Societal Construction Tool (SCT)* compiler, as part of a project on Aalborg University.

The language aims to ease the creation of *Agent-Based Modelling and Simulation (ABMS)*, via seperation of concern by encapsulating logic of agents and states.

The compiler is written in C#, and transpiles to C#.

## Repository Structure

The repository is split into two distinct projects

| Project                       | Description                                              |
| ----------------------------- | -------------------------------------------------------- |
| SctBuildTasks                 | The parser generator for unix-like systems               |
| SocietalConstructionTool      | The main project containing the compiler                 |
| SocietalConstructionToolTests | The test suite, ensuring the correctness of the compiler |

# Running the code

Download [the latest release](https://github.com/DATP4G6/sct/releases/latest).
- Here you will find releases for Linux, MacOS and Windows.

Installing the compiler in Linux can be achieved via

```sh
wget https://github.com/DATP4G6/sct/releases/download/v1.0.1/sct
chmod +x sct
./sct --help
```

## Development

- Install [.NET 8](https://dotnet.microsoft.com/en-us/download)
- The compiler uses `antlr4` as a parser generator - it can likely be installed via your package manager (`apt`, `brew`, `pacman` `choco` etc.)

On **Unix-like systems**, the parser files are **automatically generated** before building as part of the build process.

On **Windows**, the parser code must be **generated manually**:

```sh
cd SctBuildTasks
antlr4 -Dlanguage=CSharp Sct.g4 -o out -visitor -no-listener
cd ..
```

### Building, testing and running

> [!NOTE]
> The .NET compiler automatically rebuilds all required projects when running and testing unless told explicitly not to using the `--no-build` flag.

Run the tests
```sh
dotnet test
```

To run the project (`-c` outputs simulation trace to the console (stdout)):
```sh
dotnet run --project SocietalConstructionTool -- -c <path-to-sct-file-1> <...> <path-to-sct-file-n>
```

Alternatively, the `-o <output-file>` flag can be used to redirect the output to a file for further analysis.

Or with Nix with flakes enabled:
```sh
nix run .# -- -c <path-to-sct-file-1> <...> <path-to-sct-file-n>
```

### Benchmarking
Benchmarking requires the following additional dependencies:
- `hyperfine` to perform benchmarking
- Python 3.11+ to generate the benchmark files

To run the benchmarks, run the following commands:
```sh
cd benchmark
./runBenchmark.sh <size>
```

The `size` argument given to the `runBenchmark` script determines the number of benchmark files to generate and run.
Each benchmark file creates $2^n\cdot 1000$ agents (or 0 for $n=0$), where $n$ is the index of the benchmark file and simulate them for 10'000 ticks.
As it is currently not possible to compile an SCT project without running or vice versa, the first test only runs a single tick, so that the timings can adjust for the compilation time.

# Coding in SCT

See [examples/](examples/) for some simple code examples written in SCT.

## Syntax

The syntax is inspired by C, for a lot of basic statements.

#### Types

The language has the following types:

- `int`
- `float`
- `void`
- `Predicate`

> [!IMPORTANT]
> The language does not have boolean values, instead any value $\neq$ 0, is treated as true.
> Thus, boolean operators return either 0 or 1.

#### Basic Expressions

```
// I am a comment

// assignments
int x = 2;
float y = 5.0;

// boolean expressions
x = x < y;
// Operators: >, <, >=, <=, ==, !=, &&, ||

// binary operators
y = x + y;
// Operators: +, -, *, /, %

// other
y = !y; // unary NOT
y = -y; // unary minus
x = (int)y; // typecast - can only cast between int and float
```


#### Basic statements

```
if (<condition>) { <statements> }
// provide any amount of else if statements
else if (<condition>) { <statements> }
// and optionally an else statement
else { <statements> }

while (<condition>) { <statements> }

// an example function definition
// the arrow `->` indicates the return type.
// if not `void` return an expression using the `return` keyword.
function foo(int a, float b, Predicate c) -> void { <statements> }
```

#### SCT Specific Syntax


```
species Foo(int n, float i) {
  function seven() -> int {
    return 7;
  }

  decorator Bar {
    n = n + 1;
  }

  @Bar
  state Baz {
    destroy;
  }

  @Bar
  state Qux {
    float x = seven();
    enter Baz;
  }
}
```

This defines a new species with 2 fields: `n` and `i`.
`n` and `i` are only accessible within each instance of the species (agent), likewise with the function `seven()`.
Decorators and states are not directly accessible by anyone, but rather executed by the SCT runtime.

#### Predicates

The SCT type `Predicate` is used to specify a pattern matching possible agents.
```
// the predicate syntax looks as follows:
<species>::<state/?>(<field>: <value>, ...)

Predicate a = Foo::Qux(); // match any agent Foo in state Qux
Predicate a = Foo::Baz(i: 4); // match any agent Foo in state Baz having i == 4
Predicate a = Foo::?(); // match any agent Foo in any state
Predicate a = Foo::?(n: -7, i: 1); // match any agent Foo having n == -7 and i == 1
```

Predicates can be used with the builtin functions `exists()` and `count()`.
- `exists(p)` returns 1 if there exists an agent fulfilling `p`, 0 otherwise.
- `count(p)` returns an int with the amount of agents matching `p`.

#### Keywords

To create a new agent, use the `create` keyword, followed by a fully qualified agent predicate.
This means that all fields must be specified, and the state cannot be the wildcard operator `?`.
An agent can exclude itself from the succeeding iterations (ticks) using the `destroy` keyword.

To stop the simulation entirely, use the `exit` keyword. This completes the current tick, and stops afterwards.

All states must specify an end-condition, these can be either `enter` `exit` or `destroy`.
Decorators allows for less redundant code, as different states can have the same logic prepended to themselves.
It is also possible to use the end-conditions inside a decorator, which is not possible in a function.

> [!IMPORTANT]
> All SCT programs must include a `setup()` function, which must take 0 arguments and return `void`.
> This function is automatically invoked before the simulation is run, to setup the start conditions for the simulation.

#### RNG

The language also includes a RNG, which is invoked with `rand()`
`rand()` returns a float in the range `[0,1[`
It can also be seeded (to create reproducible simulations), using `seed()`, which takes an int as argument.

#### Print functions

Sometimes (mainly when debugging), it is benefitial to be able to print all the agents matching some predicate `p`.
Or printing the amount of agents matching `p`.
Both of these functionalities can be achieved with `print(p)` and `printCount(p)` respectively.

This is seen in the [examples/FizzBuzz.sct](examples/FizzBuzz.sct)

#### Syntactic Sugar

Lastly, there exists some syntactic sugar, to ease the process of writing some simulations:

- `#e` is a shorthand for `count(e)`
- `e => i` is a sharthand for `if (e) { enter i; }`

### Standard Library

The compiler includes a standard library, with the following functions:

- `exitAfterTicks(int x)`
  - exits the simulation after `x` ticks.
- `exitWhenExists(Predicate p)`
  - exits the simulation when `p` is fulfilled by some agent.
- `exitWhenNoLongerExists(Predicate p)`
  - exits the simulation when no agent fulfills `p`.
- `exitWhenMoreThan(Predicate p, int x)`
  - exits the simulation when more than `x` agents fulfill `p`.
- `exitWhenLessThan(Predicate p, int x)`
  - exits the simulation when less than `x` agents fulfill `p`.

### Output

The output of a simulation is a JSON object for each tick, ticks seperated by newlines.
SCT does not provide a way to plot the output in any way, this is instead up to the user.

[scripts/society-plot.jl](scripts/society-plot.jl) does however, include an example of how to plot a simple simulation on a graph.

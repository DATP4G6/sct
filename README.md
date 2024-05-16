![.NET status](https://github.com/DATP4G6/sct/actions/workflows/dotnet.yml/badge.svg)
![Format status](https://github.com/DATP4G6/sct/actions/workflows/dotnet-format.yml/badge.svg)

# Societal Construction Tool

Short name: _SCT_

## Repository Structure

The repository is split into two distinct projects

| Project                       | Description                                              |
| ----------------------------- | -------------------------------------------------------- |
| SctBuildTasks                 | The parser generator for unix-like systems               |
| SocietalConstructionTool      | The main project containing the compiler                 |
| SocietalConstructionToolTests | The test suite, ensuring the correctness of the compiler |

## Prerequisites

- `dotnet` v8+
- `antlr4`

On **Unix-like systems**, the parser files are **automatically generated** before building as part of the build process.

On **Windows**, the parser code must be **generated manually**:

```sh
cd SctBuildTasks
antlr4 -Dlanguage=CSharp Sct.g4 -o out -visitor
cd ..
```

## Dotnet commands

To run tests:

```sh
dotnet test
```

To run the project:

```sh
dotnet run --project SocietalConstructionTool -- -c <path-to-sct-file-1> <...> <path-to-sct-file-n>
```

Alternatively, the `-o <output-file>` flag can be used to redirect the output to a file for further analysis.

Or with Nix with flakes enabled:
```sh
nix run .# -- -c <path-to-sct-file-1> <...> <path-to-sct-file-n>
```

## Benchmarking
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

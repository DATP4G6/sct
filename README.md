![alt text](https://github.com/DATP4G6/sct/actions/workflows/dotnet.yml/badge.svg)
![alt text](https://github.com/DATP4G6/sct/actions/workflows/dotnet-format.yml/badge.svg)

# Societal Construction Tool

Short name: _SCT_

## Repository Structure

The repository is split into two distinct projects

| Project                       | Description                                              |
| ----------------------------- | -------------------------------------------------------- |
| SocietalConstructionTool      | The main project containing the compiler                 |
| SocietalConstructionToolTests | The test suite, ensuring the correctness of the compiler |

## Getting started

To start the project, follow the steps as described below.

Install the required system dependencies;

- `dotnet` v8+
- `antlr4`

Then invoke antlr4 to generate the parser files required by the rest of the project

```sh
cd SocietalConstructionTool/Compiler/Parser
antlr4 -Dlanguage=CSharp Sct.g4 -o out
```

### Dotnet commands

To run tests:

```sh
cd SocietalConstructionToolTests
dotnet test
```

To run the project, current code structure doesn't handle paths from the project root, so a directory change is required

```sh
cd SocietalConstructionTool
dotnet run
```

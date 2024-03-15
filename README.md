# Societal Construction Tool

Short name: _SCT_

## Repository Structure

The repository is split into two distinct projects

| Project                      | Description                                              |
| ---------------------------- | -------------------------------------------------------- |
| SocietalContructionTool      | The main project containing the compiler                 |
| SocietalContructionToolTests | The test suite, ensuring the correctness of the compiler |

## Getting started

To start the project, follow the steps as described below.

Install the required system dependencies;

- `dotnet` v8+
- `antlr4`

Then invoke antlr4 to generate the parser files required by the rest of the project

```sh
antlr4 -Dlanguage=CSharp ./SocietalContructionTool/parser/Sct.g4 -o ./SocietalContructionTool/parser/out
```

### Dotnet commands

To run tests:

```sh
dotnet test
```

To run the project, current code structure doesn't handle paths from the project root, so a directory change is required

```sh
cd SocietalContructionTool
dotnet run
```

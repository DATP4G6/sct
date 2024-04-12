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

To run the project, current code structure doesn't handle paths from the project root, so a directory change is required

```sh
cd SocietalConstructionTool
dotnet run
```

Or with Nix with flakes enabled:
```sh
nix run
```

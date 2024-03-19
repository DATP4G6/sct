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

On Unix-like systems, the parser files are automatically generated after cleaning as part of the build process.

On Windows, the parser code must first be generated:

```sh
cd SctBuildTasks
antlr4 -Dlanguage=CSharp Sct.g4 -o out
cd ..
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

Or with Nix with flakes enables:
```sh
nix run
```

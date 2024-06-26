﻿using System.CommandLine;

using Sct;
using Sct.Runtime.Trace;

var outputFileOption = new Option<FileInfo>(
        aliases: ["--output-file", "-o"],
        description: "The file to output to"
    );

var outputToConsoleOption = new Option<bool>(
        aliases: ["--output-to-console", "-c"],
        description: "Whether to output simulation logs to the console"
    );

var sourceFilesArgument = new Argument<List<FileInfo>>(
            "files",
            description: "The SCT source files to run"
        );

var rootCommand = new RootCommand("SCT Compiler");
rootCommand.AddOption(outputFileOption);
rootCommand.AddOption(outputToConsoleOption);
rootCommand.AddArgument(sourceFilesArgument);

rootCommand.SetHandler((sourceFiles, outputToConsole, outputFile) =>
        {
            IOutputLogger? logger = null;
            if (outputToConsole && outputFile is not null)
            {
                Console.Error.WriteLine("Cannot output to the console and a file at the same time");
                return;
            }
            if (outputFile is not null)
            {
                logger = new JsonFileLogger(outputFile.FullName);
            }
            else if (outputToConsole)
            {
                logger = new JsonConsoleLogger();
            }
            else
            {
                Console.WriteLine("Warning: No output specified");
            }

            if (sourceFiles.Count == 0)
            {
                Console.Error.WriteLine("Error: No source files specified");
                return;
            }
            _ = SctRunner.CompileAndRun(sourceFiles.Select(f => f.FullName), logger);
        }, sourceFilesArgument, outputToConsoleOption, outputFileOption);


_ = rootCommand.Invoke(args);

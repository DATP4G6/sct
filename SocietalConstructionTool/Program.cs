using System.CommandLine;

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
                Console.WriteLine("Warning: No output has been specified");
            }
            _ = SctRunner.CompileAndRun(sourceFiles.Select(f => f.FullName).ToArray(), logger);
        }, sourceFilesArgument, outputToConsoleOption, outputFileOption);


_ = rootCommand.Invoke(args);

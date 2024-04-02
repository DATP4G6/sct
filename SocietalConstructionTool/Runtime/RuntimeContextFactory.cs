using System.CommandLine;

using Sct.Runtime.Trace;

namespace Sct.Runtime
{
    public class RuntimeContextFactory
    {
        public static IRuntimeContext Create() => Create(null);

        public static IRuntimeContext Create(IOutputLogger? logger) => new RuntimeContext(new AgentHandler(), new QueryHandler([]), logger);

        /// <summary>
        /// Create the next RuntimeContext based on the current context.
        ///
        /// Agents added to agent handler will be passed to the new query handler, and agent handler is reset
        /// </summary>
        /// <param name="ctx">The previous context to update from</param>
        /// <returns></returns>
        public static IRuntimeContext CreateNext(IRuntimeContext ctx)
        {
            return new RuntimeContext(new AgentHandler(), new QueryHandler(ctx.AgentHandler.Agents), ctx.OutputLogger);
        }

        /// <summary>
        /// Create a <see cref="RuntimeContext"/> from a list of command-line arguments
        /// </summary>
        public static IRuntimeContext? CreateFromArgs(string[] args)
        {
            var outputFileOption = new Option<FileInfo?>(
                    aliases: ["--output-file", "-o"],
                    description: "The file to output to"
                );

            var rootCommand = new RootCommand("SCT Compiler");
            var printCommand = new Command("print", "Run simulation, printing each tick to the console");
            var writeCommand = new Command("write", "Run simulation and write each tick to an output file");
            writeCommand.AddOption(outputFileOption);
            rootCommand.AddCommand(printCommand);
            rootCommand.AddCommand(writeCommand);

            IRuntimeContext? ret = null;

            printCommand.SetHandler(() => ret = Create(new JsonConsoleLogger()));
            writeCommand.SetHandler((outputFile) =>
                {
                    if (outputFile is null)
                    {
                        // TODO: Figure out how to print an error to the console here, but still not break tests
                        Console.WriteLine("A destination must be given with -o <file>");
                        return;
                    }
                    ret = Create(new JsonFileLogger(outputFile.FullName));
                }, outputFileOption);

            _ = rootCommand.Invoke(args);

            return ret;
        }

        private enum OutputDestination
        {
            Console, File
        }
    }
}

using System.CommandLine;
using System.Linq;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

var entryPoint = new EntryPoint(args, "CLI tool for common Dyn activities");
return entryPoint
    .Register<ShowColoursCommand>()
    .Register<FurtleCommand>()
    .Register<FurtleArgsCommand>()
    .Register<VeryLongCommand>()
    .Execute();




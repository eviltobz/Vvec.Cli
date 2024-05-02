using System.CommandLine;
using System.Linq;
using Vvec.Cli;
using Vvec.Cli.UI;


//var strArg = new Argument<string>("str", "str blurb");
//var intArg = new Argument<int>("int", "int blurb");

//var optA = new Option<bool>("-a", "opt a blurb");
//optA.AddAlias("--OptA");
//var optB = new Option<bool>("-b", "opt b blurb");
//optB.AddAlias("--OptB");
//var optBUpper = new Option<bool>("-B", "opt b upper blurb");
//optBUpper.AddAlias("--OptBUpper");

//var cmd = new RootCommand("System.CommandLine flavoured...");
//cmd.Add(strArg);
//cmd.Add(intArg);
//cmd.Add(optA);
//cmd.Add(optB);
//cmd.Add(optBUpper);

//cmd.SetHandler((strVal, intVal, aVal, bVal, bUpperVal) =>
//    Console.WriteLine($"str:{strVal}, int:{intVal}, a:{aVal}, b:{bVal}, B:{bUpperVal}"),
//    strArg, intArg, optA, optB, optBUpper);

//cmd.Invoke(args);
//return 0;


var defaultConfig = new Config()
{
    StringValue = "my string",
    IntValue = 23
};

var entryPoint = new MultiCommandEntryPoint(args, "CLI tool for testing my CLI library.");
return entryPoint
    .WithConfig<Config>(defaultConfig)
    .Group("Interesting Functionality")
    .Register<ShowColoursCommand>()
    .Register<FurtleArgsCommand>()
    .Register<FurtleArgs2Command>()
    .Register<RunSourceGeneratorCommand>()
    .Register<ConfigCheckCommand>()
    .Group("Long help text stuff")
    .Register<FurtleCommand>()
    .Register<NoDescriptionCommand>()
    .Register<VeryLongCommand>()
    .Execute();


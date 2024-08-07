﻿using Vvec.Cli;

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
    .Register<RunSourceGeneratorCommand>()
    .Register<ConfigCheckCommand>()
    .Group("Long help text stuff")
    .Register<FurtleCommand>()
    .Register<NoDescriptionCommand>()
    .Register<VeryLongCommand>()
    .Execute();


using Vvec.Cli;
using Vvec.Prj.Logic;

return new DefaultCommandEntryPoint<PrjCommand>(args, "CLI tool for rapid access to project folders")
    .RegisterAdditionalCommand<GitCommand>(command => command
        .SubCommand<GitUpdateCommand>()
        .SubCommand<GitRevertCommand>())
    .WithConfig(Config.CreateDefault())
    .AddDependency<IIO, IO>()
    .AddDependency<IOpenDirect, OpenDirect>()
    .AddDependency<OpenFromList>()
    .AddDependency<PartialMatcher>()
    .Execute();

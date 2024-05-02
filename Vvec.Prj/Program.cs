using Vvec.Cli;
using Vvec.Prj.Logic;

return new SingleCommandEntryPoint<PrjCommand>(args, "CLI tool for rapid access to project folders")
    .WithConfig<Config>(Config.CreateDefault())
    .AddDependency<IIO, IO>()
    .AddDependency<IOpenDirect, OpenDirect>()
    .AddDependency<OpenFromList>()
    .Execute();

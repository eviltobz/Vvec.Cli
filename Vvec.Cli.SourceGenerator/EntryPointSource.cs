namespace Vvec.Cli.SourceGenerator
{
    public static class EntryPointSource
    {
        private static string CommonCommands(string type) => $@"

    public {type} AddDependency<TInterface, TImplementation>()
    {{
        initialiser.AddDependency<TInterface, TImplementation>();
        return this;
    }}

    public {type} AddDependency<T>()
    {{
        initialiser.AddDependency<T>();
        return this;
    }}

    public {type} AddDependency<T>(T instance)
    {{
        initialiser.AddDependency<T>(instance);
        return this;
    }}

    public {type} AddDependency<T>(Func<T> func)
    {{
        initialiser.AddDependency<T>(func);
        return this;
    }}

    public {type} WithConfig<TConfig>(TConfig defaultConfig = null) where TConfig : class, new()
    {{
        initialiser.WithConfig<TConfig>(defaultConfig);
        return this;
    }}

    /// <summary>
    /// Testing method, not to run as part of the main flow, but in unit tests, or diagnostic subcommands or sommat
    /// </summary>
    public (bool success, Type[] failures) CanResolveAllTypes() =>
        initialiser.CanResolveAllTypes();

    public int Execute() 
    {{
        return initialiser.Execute(Vvec.Cli.Sg.RegisterCommands);
    }}

";

        public static readonly string Source = @"
using System;
using Vvec.Cli.Arguments;

namespace Vvec.Cli;

public class SingleCommandEntryPoint<TSubCommand> where TSubCommand : ISubCommandBase
{
    private readonly Initialiser initialiser;

    public SingleCommandEntryPoint(string[] args, string description)
    {
        initialiser = new Initialiser(args, description);
        initialiser.RegisterSingleCommand<TSubCommand>();
    }

/*
    public SingleCommandEntryPoint<TSubCommand> WithConfig<TConfig>(TConfig defaultConfig = null) where TConfig : class, new()
    {
        initialiser.WithConfig<TConfig>(defaultConfig);
        return this;
    }

    public int Execute()
    {
        return initialiser.Execute(Vvec.Cli.Sg.RegisterCommands);
    }
*/" + CommonCommands("SingleCommandEntryPoint<TSubCommand>") + @"
}

public class MultiCommandEntryPoint
{
    private readonly Initialiser initialiser;

    public MultiCommandEntryPoint(string[] args, string description)
    {
        initialiser = new Initialiser(args, description);
    }

    public MultiCommandEntryPoint Group(string name)
    {
        initialiser.Group(name);
        return this;
    }

    public MultiCommandEntryPoint Register<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        initialiser.Register<TSubCommand>();
        return this;
    }" + CommonCommands("MultiCommandEntryPoint") + @"

}
";
    }
}

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

/*
public class EntryPoint // This can prolly be deleted...
{
    //private readonly Dictionary<Type, object?> dependencies = new Dictionary<Type, object?>();
    //private readonly Dictionary<Type, Func<object>> dependencyFuncs = new Dictionary<Type, Func<object>>();
    //public RootCommand rootCommand = null;

    //private readonly Introspection.Introspection meh;
    //private readonly System.CommandLine.IConsole? argumentConsole = null;
    //private readonly ArgumentParserConsole parserConsole = new ArgumentParserConsole();


    //private string[] args; // This not being readonly is a bit nasty, may wanna rethink things at some point...
    //// We only want to remove global args, not any that are registered for a command. So let's also pass in any global args in the ctor,
    //// check em, remove em from the arg list, then set this member as readonly. Create a dictionaryesque thing with globals that are
    //// present, and have a case insensitive search for them? Or, can I have a strongly typed object, and use the names from that as
    //// the flags to check for? That'd be better than a stringly typed dictionary!


    private readonly Initialiser initialiser;

    public EntryPoint(string[] args, string description)
    {
        initialiser = new Initialiser(args, description);
    }

    public EntryPoint Group(string name)
    {
        initialiser.Group(name);
        return this;
    }

    public EntryPoint AddDependency<T>()
    {
        initialiser.AddDependency<T>();
        return this;
    }

    public EntryPoint AddDependency<T>(T instance)
    {
        initialiser.AddDependency<T>(instance);
        return this;
    }

    public EntryPoint AddDependency<T>(Func<T> func)
    {
        initialiser.AddDependency<T>(func);
        return this;
    }

    public EntryPoint WithConfig<TConfig>(TConfig defaultConfig = null) where TConfig : class, new()
    {
        initialiser.WithConfig<TConfig>(defaultConfig);
        return this;
    }

    public EntryPoint Register<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        initialiser.Register<TSubCommand>();
        return this;
    }

    // Don't leave this as public, it's just whilst iterating towards extraction...
    //public T Resolve<T>() => (T)Resolve(typeof(T));


    /// <summary>
    /// Testing method, not to run as part of the main flow, but in unit tests, or diagnostic subcommands or sommat
    /// </summary>
    public (bool success, Type[] failures) CanResolveAllTypes() =>
        initialiser.CanResolveAllTypes();

    public int Execute()
    {
        return initialiser.Execute(Vvec.Cli.Sg.RegisterCommands);
    }
}
*/
";
    }
}

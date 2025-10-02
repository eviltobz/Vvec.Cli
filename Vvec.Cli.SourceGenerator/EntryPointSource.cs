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

public class DefaultCommandEntryPoint<TSubCommand> where TSubCommand : ISubCommandBase
{
    private readonly Initialiser initialiser;

    public DefaultCommandEntryPoint(string[] args, string description)
    {
        initialiser = new Initialiser(args, description);
        initialiser.RegisterDefaultCommand<TSubCommand>();
    }

    public DefaultCommandEntryPoint<TSubCommand> RegisterAdditionalCommand<TAdditionalCommand>() where TAdditionalCommand : ISubCommandBase
    {
        initialiser.Register<TAdditionalCommand>(null);
        return this;
    }

    public DefaultCommandEntryPoint<TSubCommand> RegisterAdditionalCommand<TAdditionalCommand>(Action<SubCommandRegister> subCommandRegistration) where TAdditionalCommand : ISubCommandBase
    {
        var subCommands = new SubCommandRegister(initialiser);
        subCommandRegistration(subCommands);
        initialiser.Register<TAdditionalCommand>(subCommands);
        return this;
    }

" + CommonCommands("DefaultCommandEntryPoint<TSubCommand>") + @"
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
        initialiser.Register<TSubCommand>(null);
        return this;
    }

    public MultiCommandEntryPoint Register<TSubCommand>(Action<SubCommandRegister> subCommandRegistration) where TSubCommand : ISubCommandBase
    {
        var subCommands = new SubCommandRegister(initialiser);
        subCommandRegistration(subCommands);
        initialiser.Register<TSubCommand>(subCommands);
        return this;
    }

    " + CommonCommands("MultiCommandEntryPoint") + @"

}
";
    }
}

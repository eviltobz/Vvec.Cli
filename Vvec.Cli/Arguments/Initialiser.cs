using System.CommandLine;
using Microsoft.CodeAnalysis;
using Vvec.Cli.Arguments.ParserConsole;
using Vvec.Cli.Config;
using Vvec.Cli.UI;
using IConsole = Vvec.Cli.UI.IConsole;

namespace Vvec.Cli.Arguments;

public partial class Initialiser
{
    private static readonly object ResolvedToNull = new object();
    private static readonly IConsole cons = VConsole.Instance;
    private static readonly IConsole verbose = cons.Verbose;

    private readonly Dictionary<Type, object?> dependencies = new Dictionary<Type, object?>();
    private readonly Dictionary<Type, Func<object>> dependencyFuncs = new Dictionary<Type, Func<object>>();
    private readonly Introspection.CommandFactory commandFactory;
    private readonly System.CommandLine.IConsole? argumentConsole = null;
    private readonly ArgumentParserConsole parserConsole = new ArgumentParserConsole();
    private readonly Dictionary<Type, Command> RegisteredSubCommands = new Dictionary<Type, Command>();


    public RootCommand rootCommand = null;
    private bool IsSingleCommand = false;
    private Type? ConfigCommandType = null;
    private Action? DeferredRegisterConfig = null;

    private string[] args; // This not being readonly is a bit nasty, may wanna rethink things at some point...
                           // We only want to remove Vvec.Cli-specific args, not any that are registered for a command. So let's also pass in any Vvec.Cli-specific args in the ctor,
                           // check em, remove em from the arg list, then set this member as readonly. Create a dictionaryesque thing with Vvec.Cli-specifics that are
                           // present, and have a case insensitive search for them? Or, can I have a strongly typed object, and use the names from that as
                           // the flags to check for? That'd be better than a stringly typed dictionary!

    public Initialiser(string[] args, string description)
    {

        this.args = args;
        if (CheckAndRemoveVvecCliOption("--verbose"))
            VConsole.SetVerbose();

        argumentConsole = CheckAndRemoveVvecCliOption("--dc")
            ? null
            : parserConsole;

        this.rootCommand = new RootCommand(description);

        AddDependency(VConsole.Instance);

        commandFactory = new Introspection.CommandFactory(this);
    }

    public Initialiser Group(string name)
    {
        parserConsole.AddGroup(name);
        return this;
    }

    private bool CheckAndRemoveVvecCliOption(string optionName)
    {
        var option = optionName.ToUpper();
        if (!args.Any(a => a.ToUpper() == option))
            return false;

        this.args = args.Where(a => a.ToUpper() != option).ToArray();
        return true;
    }

    public Initialiser AddDependency<TInterface, TImplementation>()
    {
        dependencies.Add(typeof(TInterface), null);
        dependencyFuncs.Add(typeof(TInterface), () => Construct(typeof(TImplementation)));
        return this;
    }

    public Initialiser AddDependency<T>()
    {
        dependencies.Add(typeof(T), null);
        dependencyFuncs.Add(typeof(T), () => Construct(typeof(T)));
        return this;
    }

    public Initialiser AddDependency<T>(T instance)
    {
        dependencies.Add(typeof(T), instance);
        return this;
    }

    public Initialiser AddDependency<T>(Func<T> func)
    {
        dependencies.Add(typeof(T), null);
        dependencyFuncs.Add(typeof(T), () => func());
        return this;
    }

    public Initialiser Register<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        AddDependency<TSubCommand>();
        Command command = commandFactory.SetUpSubCommand<TSubCommand>(verbose);
        parserConsole.AddCommand(command.Name);
        rootCommand.Add(command);
        RegisteredSubCommands.Add(typeof(TSubCommand), command);
        return this;
    }

    public Initialiser RegisterSingleCommand<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        IsSingleCommand = true;
        AddDependency<TSubCommand>();
        rootCommand = commandFactory.SetUpRootCommand<TSubCommand>(verbose);
        parserConsole.AddCommand(rootCommand.Name);
        RegisteredSubCommands.Add(typeof(TSubCommand), rootCommand);
        return this;
    }

    public void WithConfig<TConfig>(TConfig? defaultConfig = null) where TConfig : class, new()
    {
        ConfigCommandType = typeof(ConfigCommand<TConfig>);
        var configStore = new ConfigStore<TConfig>(defaultConfig, cons);
        AddDependency<TConfig>(configStore.Read);
        AddDependency<ConfigStore<TConfig>>(configStore);
        DeferredRegisterConfig = () => Register<ConfigCommand<TConfig>>();
    }

    // Don't leave this as public, it's just whilst iterating towards extraction...
    public T Resolve<T>() => (T)Resolve(typeof(T));

    private object Resolve(Type type)
    {
        var success = dependencies.TryGetValue(type, out var instance);
        if (!success)
            throw new Exception($"Dependency not found: {type.FullName}");

        if (instance == ResolvedToNull)
            return null;

        if (instance is not null)
            return instance;

        var func = dependencyFuncs[type];
        instance = func() ?? ResolvedToNull;

        dependencies[type] = instance;

        return instance;
    }

    private object Construct(Type type)
    {
        object? instance;
        var ctors = type.GetConstructors();
        if (ctors.Length != 1)
            throw new Exception($"Exactly 1 public constructor required, but found {ctors.Length} for type: {type.FullName}");

        var ctor = ctors[0];

        var paramInfos = ctor.GetParameters();
        var paramInstances = new object[paramInfos.Length];

        for (int i = 0; i < paramInfos.Length; i++)
            paramInstances[i] = Resolve(paramInfos[i].ParameterType);

        instance = ctor.Invoke(paramInstances);
        return instance;
    }

    /// <summary>
    /// Testing method, not to run as part of the main flow, but in unit tests, or diagnostic subcommands or sommat
    /// </summary>
    public (bool success, Type[] failures) CanResolveAllTypes()
    {
        throw new NotImplementedException("TODO: Add check for resolving all registered types");
    }

    // This is setting up the config command in a hardcoded manner, so changes to it aren't automagically reflected.
    // It'd be nice if this were source-genned too, but the source-gen is running on the app we're building, but the
    // config command is in the library that we're importing along with this. Depending on what you can get hold of
    // to look at, it _might_ be possible to source-gen it, in a more reflectiony over the included library kind of
    // way, but it might not...
    public int Execute(Action<Dictionary<Type, Command>, Func<Type, object>> registerCommandExecution)
    {
        if (ConfigCommandType is not null)
        {
            if (IsSingleCommand)
                parserConsole.NoGroups();
            else
                Group("Misc");

            DeferredRegisterConfig!();
            var command = RegisteredSubCommands[ConfigCommandType];

            var KeyArg = new Argument<string>("key", () => string.Empty, "Name (or camel-cased initials) of the config property");
            command.AddArgument(KeyArg);

            var ValueArg = new Argument<string>("value", () => string.Empty, "The value to store");
            command.AddArgument(ValueArg);

            var editOpt = new Option<bool>(new[] { "-e", "--edit" }, "Edit config file in Vim (provided key/values will be ignored)");
            command.AddOption(editOpt);

            command.SetHandler((keyValue, valueValue, editValue) =>
            {
                var instance = (ISubCommand)Resolve(ConfigCommandType);
                var props = instance.GetType().GetProperties();
                props.Single(p => p.Name == "Key").SetValue(instance, keyValue);
                props.Single(p => p.Name == "Value").SetValue(instance, valueValue);
                props.Single(p => p.Name == "Edit").SetValue(instance, editValue);
                instance.Execute();
            }, KeyArg, ValueArg, editOpt);
        }

        registerCommandExecution(RegisteredSubCommands, Resolve);

        return rootCommand.Invoke(args, argumentConsole);
    }
}

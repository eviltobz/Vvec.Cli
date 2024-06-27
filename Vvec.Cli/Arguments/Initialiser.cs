using Microsoft.CodeAnalysis; // Uninstall this package from my nugnugs if it doesn't work cos this aint netstandard 2.0...
using System.CommandLine;
using System.CommandLine.Binding;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Vvec.Cli.Arguments.Introspection;
using Vvec.Cli.Arguments.ParserConsole;
using Vvec.Cli.Config;
using Vvec.Cli.UI;
using IConsole = Vvec.Cli.UI.IConsole;

namespace Vvec.Cli.Arguments;

public partial class Initialiser
{
    private static readonly object ResolvedToNull = new object();
    private readonly Dictionary<Type, object?> dependencies = new Dictionary<Type, object?>();
    private readonly Dictionary<Type, Func<object>> dependencyFuncs = new Dictionary<Type, Func<object>>();
    public RootCommand rootCommand = null;

    private readonly Introspection.Introspection meh;
    private readonly System.CommandLine.IConsole? argumentConsole = null;
    private readonly ArgumentParserConsole parserConsole = new ArgumentParserConsole();

    private bool IsSingleCommand = false;


    private string[] args; // This not being readonly is a bit nasty, may wanna rethink things at some point...
                           // We only want to remove global args, not any that are registered for a command. So let's also pass in any global args in the ctor,
                           // check em, remove em from the arg list, then set this member as readonly. Create a dictionaryesque thing with globals that are
                           // present, and have a case insensitive search for them? Or, can I have a strongly typed object, and use the names from that as
                           // the flags to check for? That'd be better than a stringly typed dictionary!


    private static readonly IConsole cons = VConsole.Instance;
    private static readonly IConsole verbose = cons.Verbose;

    [Obsolete("I need to be phased out, & rootCommand made private readonly")]
    public Initialiser()
    {
        // REMEMBER I NEED KILLING, NOT EXTENDING...
        //SG();

        meh = new Introspection.Introspection(this);
        argumentConsole = parserConsole;
    }

    public Initialiser Group(string name)
    {
        parserConsole.AddGroup(name);
        return this;
    }

    private bool CheckAndRemoveGlobalOption(string optionName)
    {
        var option = optionName.ToUpper();
        if (!args.Any(a => a.ToUpper() == option))
            return false;

        this.args = args.Where(a => a.ToUpper() != option).ToArray();
        return true;
    }

    public Initialiser(string[] args, string description)
    {

        this.args = args;
        if (CheckAndRemoveGlobalOption("--verbose"))
            VConsole.SetVerbose();

        argumentConsole = CheckAndRemoveGlobalOption("--dc")
            ? null
            : parserConsole; // this is a bit sucky  - later me: Is it? Why? Not sure what I was thinking when I wrote that...

        this.rootCommand = new RootCommand(description);

        AddDependency(VConsole.Instance);

        meh = new Introspection.Introspection(this);
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
        //var cons = VConsole.Instance;
        //var verbose = cons.Verbose;
        //cons.WriteLine("Registering command: ", typeof(TSubCommand).Name.InYellow());
        AddDependency<TSubCommand>();

        Command command = meh.SetUpSubCommand<TSubCommand>(verbose);

        parserConsole.AddCommand(command.Name);

        //command.SetHandler(Action<string, bool, int> (arg1, arg2, arg3) => Console.WriteLine("meh"));

        rootCommand.Add(command);

        RegisteredSubCommands.Add(typeof(TSubCommand), command);
        return this;
    }

    public Initialiser RegisterSingleCommand<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        IsSingleCommand = true;

        AddDependency<TSubCommand>();

        rootCommand = meh.SetUpRootCommand<TSubCommand>(verbose);

        parserConsole.AddCommand(rootCommand.Name);


        RegisteredSubCommands.Add(typeof(TSubCommand), rootCommand);
        return this;
    }

    //private Type ConfigType = null;
    private Type? ConfigCommandType = null;
    private Action? DeferredRegisterConfig = null;

    public void WithConfig<TConfig>(TConfig? defaultConfig = null) where TConfig : class, new()
    {
        //ConfigType = typeof(TConfig);
        ConfigCommandType = typeof(ConfigCommand<TConfig>);
        var configStore = new ConfigStore<TConfig>(defaultConfig, cons);
        AddDependency<TConfig>(configStore.Read);
        AddDependency<ConfigStore<TConfig>>(configStore);

        DeferredRegisterConfig = () => Register<ConfigCommand<TConfig>>();
    }

    private readonly Dictionary<Type, Command> RegisteredSubCommands = new Dictionary<Type, Command>();



    //public Initialiser Register<TSubCommand>() where TSubCommand : ISubCommandAsync
    //{
    //    VConsole.Instance.WriteLine("Registering command: ", typeof(TSubCommand).Name.InYellow());
    //    AddDependency<TSubCommand>();
    //    var nameProp = typeof(TSubCommand).GetProperty(nameof(ISubCommand.Name), BindingFlags.Public | BindingFlags.Static);
    //    var descriptionProp = typeof(TSubCommand).GetProperty(nameof(ISubCommand.Description), BindingFlags.Public | BindingFlags.Static);
    //    var command = new Command(nameProp!.GetValue(null)!.ToString()!, descriptionProp!.GetValue(null)!.ToString());
    //    command.SetHandler(() => Resolve<TSubCommand>().Execute().Wait);
    //    rootCommand.Add(command);
    //    return this;
    //}

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

        //_ = dependencyFuncs.TryGetValue(type, out var func);
        //if (func is not null)
        //    instance = func();
        //else
        //    instance = Construct(type);

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
        return (false, Array.Empty<Type>());
    }
    //public partial void RegisterCommands(Dictionary<Type, Command> registeredCommands);

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
                //var instance = Resolve<ConfigCommand>();
                var instance = (ISubCommand)Resolve(ConfigCommandType);
                var props = instance.GetType().GetProperties();
                props.Single(p => p.Name == "Key").SetValue(instance, keyValue);
                props.Single(p => p.Name == "Value").SetValue(instance, valueValue);
                props.Single(p => p.Name == "Edit").SetValue(instance, editValue);
                instance.Execute();
            }, KeyArg, ValueArg, editOpt);
        }


        registerCommandExecution(RegisteredSubCommands, Resolve);

        //if (CheckAndRemoveGlobalOption("-dc"))
        //    return rootCommand.Invoke(args);
        //else
        //    return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());
        return rootCommand.Invoke(args, argumentConsole);

        //if (args.Length > 0 && args[0] == "-dc")

        //    return rootCommand.Invoke(args.Skip(1).ToArray());
        //else
        //    // This rootCommand needs to disappear, as does the console. Need a simple start method on Initialiser. And the args should go into the Initialiser ctor, so a verbose flag (and any other global stuff) can be set up right at the start.
        //    return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());
    }
}

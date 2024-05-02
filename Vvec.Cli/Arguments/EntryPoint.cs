using Microsoft.CodeAnalysis; // Uninstall this package from my nugnugs if it doesn't work cos this aint netstandard 2.0...
using System.CommandLine;
using System.CommandLine.Binding;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Vvec.Cli.Arguments.Introspection;
using Vvec.Cli.Arguments.ParserConsole;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments;
public partial class EntryPoint
{
    private readonly Dictionary<Type, object?> dependencies = new Dictionary<Type, object?>();
    private readonly Dictionary<Type, Func<object>> dependencyFuncs = new Dictionary<Type, Func<object>>();
    public RootCommand rootCommand = null;

    private readonly Introspection.Introspection meh;
    private readonly System.CommandLine.IConsole? argumentConsole = null;
    private readonly ArgumentParserConsole parserConsole = new ArgumentParserConsole();


    private string[] args; // This not being readonly is a bit nasty, may wanna rethink things at some point...
    // We only want to remove global args, not any that are registered for a command. So let's also pass in any global args in the ctor,
    // check em, remove em from the arg list, then set this member as readonly. Create a dictionaryesque thing with globals that are
    // present, and have a case insensitive search for them? Or, can I have a strongly typed object, and use the names from that as
    // the flags to check for? That'd be better than a stringly typed dictionary!



    [Obsolete("I need to be phased out, & rootCommand made private readonly")]
    public EntryPoint()
    {
        // REMEMBER I NEED KILLING, NOT EXTENDING...
        //SG();

        meh = new Introspection.Introspection(this);
        argumentConsole = parserConsole;
    }

    public EntryPoint Group(string name)
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

    public EntryPoint(string[] args, string description)
    {

        this.args = args;
        if (CheckAndRemoveGlobalOption("--verbose"))
            VConsole.SetVerbose();

        argumentConsole = CheckAndRemoveGlobalOption("--dc")
            ? null
            : parserConsole; // this is a bit sucky

        rootCommand = new RootCommand(description);

        AddDependency(VConsole.Instance);

        meh = new Introspection.Introspection(this);
    }


    public EntryPoint AddDependency<T>()
    {
        dependencies.Add(typeof(T), null);
        dependencyFuncs.Add(typeof(T), () => Construct(typeof(T)));
        return this;
    }

    public EntryPoint AddDependency<T>(T instance)
    {
        dependencies.Add(typeof(T), instance);
        return this;
    }

    public EntryPoint AddDependency<T>(Func<T> func)
    {
        dependencies.Add(typeof(T), null);
        dependencyFuncs.Add(typeof(T), () => func());
        return this;
    }

    public EntryPoint Register<TSubCommand>() where TSubCommand : ISubCommandBase
    {
        var cons = VConsole.Instance;
        var verbose = cons.Verbose;
        //cons.WriteLine("Registering command: ", typeof(TSubCommand).Name.InYellow());
        AddDependency<TSubCommand>();

        Command command = meh.SetUpCommand<TSubCommand>(verbose);

        parserConsole.AddCommand(command.Name);

        rootCommand.Add(command);
        return this;
    }



    //public EntryPoint Register<TSubCommand>() where TSubCommand : ISubCommandAsync
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

        if (instance is not null)
            return instance;

        //_ = dependencyFuncs.TryGetValue(type, out var func);
        //if (func is not null)
        //    instance = func();
        //else
        //    instance = Construct(type);

        var func = dependencyFuncs[type];
        instance = func();

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

    public int Execute()
    {
        //if (CheckAndRemoveGlobalOption("-dc"))
        //    return rootCommand.Invoke(args);
        //else
        //    return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());
        return rootCommand.Invoke(args, argumentConsole);

        //if (args.Length > 0 && args[0] == "-dc")

        //    return rootCommand.Invoke(args.Skip(1).ToArray());
        //else
        //    // This rootCommand needs to disappear, as does the console. Need a simple start method on EntryPoint. And the args should go into the EntryPoint ctor, so a verbose flag (and any other global stuff) can be set up right at the start.
        //    return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());
    }
}

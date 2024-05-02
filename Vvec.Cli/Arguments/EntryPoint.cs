using Microsoft.CodeAnalysis; // Uninstall this package from my nugnugs if it doesn't work cos this aint netstandard 2.0...
using System.CommandLine;
using System.CommandLine.Binding;
using System.Reflection;
using System.Reflection.Emit;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments;
public partial class EntryPoint
{
    private readonly Dictionary<Type, object?> dependencies = new Dictionary<Type, object?>();
    private readonly Dictionary<Type, Func<object>> dependencyFuncs = new Dictionary<Type, Func<object>>();
    public RootCommand rootCommand = null;


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
        //if (args.Any(a => a.ToUpper() == "-VERBOSE"))
        //{
        //    VConsole.SetVerbose();
        //    this.args = args.Where(a => a.ToUpper() != "-VERBOSE").ToArray();
        //}
        //else
        //    this.args = args;

        this.args = args;
        if (CheckAndRemoveGlobalOption("-verbose"))
            VConsole.SetVerbose();

        rootCommand = new RootCommand(description);

        System.Console.WriteLine("In ctor");
        //SG();
        AddDependency<Vvec.Cli.UI.IConsole>(VConsole.Instance);

    }

    //public partial void SG();
    //{
    //    System.Console.WriteLine("In static code");
    //}

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
        var nameProp = TSubCommand.Name; // typeof(TSubCommand).GetProperty(nameof(ISubCommand.Name), BindingFlags.Public | BindingFlags.Static);
        var descriptionProp = TSubCommand.Description; // typeof(TSubCommand).GetProperty(nameof(ISubCommand.Description), BindingFlags.Public | BindingFlags.Static);
        //var command = new Command(nameProp!.GetValue(null)!.ToString()!, descriptionProp!.GetValue(null)!.ToString());
        var command = new Command(nameProp, descriptionProp);
        var bob = typeof(TSubCommand);
        //var commandX = Resolve<TSubCommand>();
        var interfaces = typeof(TSubCommand).GetInterfaces();

        //var cType = commandX.GetType();
        //var isGen = commandX.GetType().IsGenericType;

        //foreach (var @interface in interfaces.Where(i => i.IsConstructedGenericType))
        //{
        //    cons.WriteLine(@interface.FullName);
        //    cons.WriteLine(@interface.Name);

        //    var open = typeof(ISubCommandWithArguments<>);

        //    var e = @interface.IsSubclassOfRawGeneric(open);

        //    var a = open.IsAssignableFrom(@interface);
        //    var b = @interface.IsAssignableFrom(open);

        //    var c = open.IsAssignableTo(@interface);
        //    var d = @interface.IsAssignableTo(open);

        //    cons.WriteLine(@interface.Name.InYellow(), " Assignables: ", a, b, c, d, e);
        //}
        //foreach (var @interface in interfaces)
        //{
        //    var open = typeof(ISubCommandWithArguments);
        //    //var a = open.IsAssignableFrom(@interface);
        //    var b = @interface.IsAssignableFrom(open);

        //    //cons.WriteLine(@interface.Name.InYellow(), " Assignables: ", a, b); //, c, d);

        //    if (b)
        //    {
        //        var prop = bob.GetProperty(nameof(ISubCommandWithArguments.Modifiers));
        //        var modifiers = (IEnumerable<Symbol>)(prop.GetGetMethod().Invoke(null, null));
        //    }

        //}

        var cGen = interfaces.FirstOrDefault(i => i.IsConstructedGenericType);
        //if (cGen is not null)
        //{
        //    var open = typeof(ISubCommandWithArguments<>);

        //    var a = open.IsAssignableFrom(cGen);
        //    var b = cGen.IsAssignableFrom(open);

        //    cons.WriteLine(cGen.Name.InYellow(), " Assignables: ", a, b); //, c, d);
        //}


        var genericArgType = typeof(Argument<>);

        if (interfaces.Contains(typeof(ISubCommand)))
            command.SetHandler(() => ((ISubCommand)Resolve<TSubCommand>()).Execute());
        else if (interfaces.Contains(typeof(ISubCommandAsync)))
            command.SetHandler(() => ((ISubCommandAsync)Resolve<TSubCommand>()).Execute().Wait());
        else if (cGen.GetGenericTypeDefinition() == typeof(ISubCommand<>))
        {
            var args = cGen.GenericTypeArguments.First();
            var fields = new List<(Symbol symbol, PropertyInfo prop)>();


            //cons.WriteLine(def);
            //cons.WriteLine("Args:", args);
            var props = args.GetProperties();
            foreach (var prop in props)
            {
                var arg = prop.GetCustomAttribute<ArgAttribute>();
                if (arg is not null)
                {
                    //cons.WriteLine("  [arg]", prop.Name, ", name:", arg.Name, ", desc:", arg.Description);
                    var argType = genericArgType.MakeGenericType(new[] { prop.PropertyType });
                    var ctor = argType.GetConstructor(Type.EmptyTypes);
                    var theArg = (Argument)ctor.Invoke(new object[0]);
                    theArg.Name = arg.Name;
                    theArg.Description = arg.Description;
                    //cons.WriteLine($"-found {ctors.Length} constructors for Argument<>");
                    //foreach (var ctor in ctors)
                    //{
                    //    var parms = ctor.GetParameters();
                    //    cons.Write($"--{ctor.Name}[cgp:{ctor.ContainsGenericParameters}](");
                    //    foreach (var parm in parms)
                    //    {
                    //        cons.Write(parm.ParameterType.Name, " ", parm.Name, ", ");
                    //    }
                    //    cons.WriteLine(")");
                    //}
                    command.AddArgument(theArg);
                    fields.Add((theArg, prop));

                    continue;
                }
                //cons.WriteLine("  [no attr]", prop.Name);
            }

            var handlerX = typeof(Handler).GetMethods();
            //cons.WriteLine(handlerX);
            //cons.WriteLine("Registering command: ", typeof(TSubCommand).Name.InYellow());
            verbose.WriteLine("Found ", fields.Count.InYellow(), " symbols for ", typeof(TSubCommand).Name.InYellow(), " to load into a ", args.Name.InYellow(), ":");
            foreach (var field in fields)
            {
                verbose.WriteLine($"  {field.symbol.Name}, {field.prop.Name}, {field.prop.PropertyType}");
            }
            verbose.WriteLine("Now I just need to codegen a function to take all of them as args, and load em into the type, then hook that up to the subcommand. Simples".InDarkRed());
        }


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
        if (CheckAndRemoveGlobalOption("-dc"))
            return rootCommand.Invoke(args);
        else
            return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());

        //if (args.Length > 0 && args[0] == "-dc")

        //    return rootCommand.Invoke(args.Skip(1).ToArray());
        //else
        //    // This rootCommand needs to disappear, as does the console. Need a simple start method on EntryPoint. And the args should go into the EntryPoint ctor, so a verbose flag (and any other global stuff) can be set up right at the start.
        //    return rootCommand.Invoke(args, new Vvec.Cli.Arguments.ArgumentParserConsole());
    }
}

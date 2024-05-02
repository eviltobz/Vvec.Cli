using System.CommandLine;
using System.Reflection;
using Microsoft.CodeAnalysis; // Uninstall this package from my nugnugs if it doesn't work cos this aint netstandard 2.0...
using Vvec.Cli.UI;


namespace Vvec.Cli.Arguments.Introspection
{
    /// <summary>
    /// This is a holding spot for my current reflection code for wiring stuff up. This would be a good candidate to convert into source generator code.
    /// For the initial dev work, I'm thinking that reflection might be easier, so I can iterate on stuff quicker. But once it's done, turning it into
    /// a source generator would give quicker code at startup (granted, it's not noticeably slow with reflection at the time of writing) but also be a
    /// useful learning experience for getting one up & running all properlike.
    /// </summary>
    public class Introspection
    {
        private Initialiser initialiser;

        public Introspection(Initialiser initialiser)
        {
            this.initialiser = initialiser;
        }

        public Command SetUpCommand<TSubCommand>(UI.IConsole verbose) where TSubCommand : ISubCommandBase
        {
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
                command.SetHandler(() => ((ISubCommand)initialiser.Resolve<TSubCommand>()).Execute());
            else if (interfaces.Contains(typeof(ISubCommandAsync)))
                command.SetHandler(() => ((ISubCommandAsync)initialiser.Resolve<TSubCommand>()).Execute().Wait());
            else if (cGen.GetGenericTypeDefinition() == typeof(ISubCommand<>))
            {
                var args = cGen.GenericTypeArguments.First();
                var fields = new List<(Symbol symbol, PropertyInfo prop)>();


                //cons.WriteLine(def);
                //cons.WriteLine("Args:", args);
                //var props = args.GetProperties();
                //foreach (var prop in props)
                //{
                //    var arg = prop.GetCustomAttribute<ArgAttribute>();
                //    if (arg is not null)
                //    {
                //        //cons.WriteLine("  [arg]", prop.Name, ", name:", arg.Name, ", desc:", arg.Description);
                //        var argType = genericArgType.MakeGenericType(new[] { prop.PropertyType });
                //        var ctor = argType.GetConstructor(Type.EmptyTypes);
                //        var theArg = (Argument)ctor.Invoke(new object[0]);
                //        theArg.Name = arg.Name;
                //        theArg.Description = arg.Description;
                //        //cons.WriteLine($"-found {ctors.Length} constructors for Argument<>");
                //        //foreach (var ctor in ctors)
                //        //{
                //        //    var parms = ctor.GetParameters();
                //        //    cons.Write($"--{ctor.Name}[cgp:{ctor.ContainsGenericParameters}](");
                //        //    foreach (var parm in parms)
                //        //    {
                //        //        cons.Write(parm.ParameterType.Name, " ", parm.Name, ", ");
                //        //    }
                //        //    cons.WriteLine(")");
                //        //}
                //        command.AddArgument(theArg);
                //        fields.Add((theArg, prop));

                //        continue;
                //    }

                //    //var opt = prop.GetCustomAttribute<OptAttribute>();
                //    //if (opt is not null)
                //    //{
                //    //    //cons.WriteLine("  [opt]", prop.Name, ", name:", opt.Name, ", desc:", opt.Description);
                //    //    var argType = genericArgType.MakeGenericType(new[] { prop.PropertyType });
                //    //    var ctor = argType.GetConstructor(Type.EmptyTypes);
                //    //    var theArg = (Argument)ctor.Invoke(new object[0]);
                //    //    theArg.Name = opt.Name;
                //    //    theArg.Description = opt.Description;
                //    //    //cons.WriteLine($"-found {ctors.Length} constructors for Argument<>");
                //    //    //foreach (var ctor in ctors)
                //    //    //{
                //    //    //    var parms = ctor.GetParameters();
                //    //    //    cons.Write($"--{ctor.Name}[cgp:{ctor.ContainsGenericParameters}](");
                //    //    //    foreach (var parm in parms)
                //    //    //    {
                //    //    //        cons.Write(parm.ParameterType.Name, " ", parm.Name, ", ");
                //    //    //    }
                //    //    //    cons.WriteLine(")");
                //    //    //}
                //    //    command.AddArgument(theArg);
                //    //    fields.Add((theArg, prop));

                //    //    continue;
                //    //}



                //    //cons.WriteLine("  [no attr]", prop.Name);
                //}

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

            return command;
        }

    }
}

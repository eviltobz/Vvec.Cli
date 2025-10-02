using System.Text;

namespace Vvec.Cli.SourceGenerator
{
    public static class RegistrationSourceGenerationHelper
    {
        public static string XGenerateClassStuff(List<ClassToRegister> classesToRegister)
        {
            return
@"using System.CommandLine;

namespace Vvec.Cli
{
    public partial class Sg
    {
        public static void RegisterCommands(Dictionary<Type, Command> registeredCommands, Func<Type, object> resolver) // where T:ISubCommand
        {
            Console.WriteLine(""Just ignore me"");
        }
    }
}";
        }


        public static string GenerateClassStuff(List<ClassToRegister> classesToRegister)
        {
            var sb = new StringBuilder();
            sb.Append(
@"using System.CommandLine;
using System;
using System.Collections.Generic;
using Vvec.Cli.Arguments;

namespace Vvec.Cli
{
    public class Sg
    {

    internal const char Escape1 = (char)27;
    internal const char Escape2 = '[';
    internal const char Terminator = 'm';
    private static readonly string Escape = new string([Escape1, Escape2]);
    private static readonly string RED = Escape + ""91m"";
    private static readonly string DARKRED = Escape + ""31m"";
    private static readonly string DEFAULT = Escape + ""0m"";


        public static void RegisterCommands(Dictionary<Type, Command> registeredCommands, Func<Type, object> resolver) // where T:ISubCommand
        {
            //Command command = null;
");
            WriteDebuggery(classesToRegister, sb);

            foreach (var registration in classesToRegister)
            {
                if (registration.Arguments.Any())
                {
                    WriteCommandWithArgs(sb, registration);
                }
                else
                {
                    WriteCommandWithoutArgs(sb, registration);
                }
            }

            sb.Append(@"
        }
    }
}");

            return sb.ToString();
        }

        private static string GetCommand(string name) =>
            @"var found = registeredCommands.TryGetValue(typeof(" + name + @"), out var command);
            if(!found)
            {
              Console.WriteLine($""{RED}A command called '{DARKRED}" + name + @"{RED}' was found but has not been registered."" + Environment.NewLine +
                $""(Maybe I should auto register any unregistered commands in alphabetical order when 'execute' is called instead of bailing...){DEFAULT}"");
              Environment.Exit(1);
            }";

        private static void WriteCommandWithoutArgs(StringBuilder sb, ClassToRegister registration)
        {
            //sb.AppendLine(@"
            //{
            //    var command = registeredCommands[typeof(" + registration.Name + @")];
            sb.AppendLine(@"
            {
                " + GetCommand(registration.Name) + @"
                command.SetHandler(() =>
                {
                    // It'd be _nice_ to do IoC resolve in SourceGen rather than reflection...
                    // It is quite an undertaking though. We have the source for the cli app, but any
                    // dependency libraries that we want to register won't be in the compilation, so
                    // it'd need to mix in compile-time reflection and source-genny code to build it all
                    // Let's hold off on that for now...
                    var implementation = (" + registration.Name + ")resolver(typeof(" + registration.Name + @"));
                    " + (registration.IsAsync ? "implementation.Execute().Wait();" : "implementation.Execute();") + @"
                });
            }");
        }

        private static void WriteCommandWithArgs(StringBuilder sb, ClassToRegister registration)
        {
            //sb.AppendLine(@"
            //{
            //    var command = registeredCommands[typeof(" + registration.Name + @")];");
            sb.AppendLine(@"
            {
                " + GetCommand(registration.Name));

            foreach (var arg in registration.Arguments)
            {
                sb.AppendLine(arg.GenerateRegistration());
            }

            sb.Append(@"
                command.SetHandler((");
            for (var i = 0; i < registration.Arguments.Length; i++)
            {
                var arg = registration.Arguments[i];
                sb.Append(arg.Name + "Value");
                if (i < registration.Arguments.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            WriteHandlerWithArgs_WithReflection(sb, registration);
            //WriteHandlerWithArgs_NoReflection(sb, registration);
        }

        private static void WriteHandlerWithArgs_WithReflection(StringBuilder sb, ClassToRegister registration)
        {
            // We need to do IoC resolve in SourceGen rather than reflection...
            sb.Append(@") =>
                {
                    // We need to do IoC resolve in SourceGen rather than reflection...
                    var instance = (" + registration.Name + ")resolver(typeof(" + registration.Name + @"));
                    var props = instance.GetType().GetProperties();");
            foreach (var arg in registration.Arguments)
            {
                sb.Append(@"
                    props.Single(p => p.Name == """ + arg.Name + @""").SetValue(instance, " + arg.Name + "Value);");
            }
            if(registration.IsAsync)
                sb.Append(@"
                    instance.Execute().Wait();
                }");
            else
                sb.Append(@"
                    instance.Execute();
                }");
            sb.Append(@", ");
            sb.Append(string.Join(", ", registration.Arguments.Select(a => a.Name)))
                .AppendLine(@");
            }");
        }

        private static void WriteHandlerWithArgs_NoReflection(StringBuilder sb, ClassToRegister registration)
        {
            // We need to do proper IoC here - this is just hardcoded
            sb.Append(@") =>
                {
                    // We need to do proper IoC here - this is just hardcoded
                    var instance = new " + registration.Name + @"(Vvec.Cli.UI.VConsole.Instance)
                    {");
            foreach (var arg in registration.Arguments)
            {
                sb.Append(@"
                        " + arg.Name + " = " + arg.Name + "Value,");
            }
            sb.Append(@"
                    };
                    instance.Execute();
                }");
            sb.Append(@", ");
            sb.Append(string.Join(", ", registration.Arguments.Select(a => a.Name)))
                .AppendLine(@");
            }");
        }

        private static void WriteDebuggery(List<ClassToRegister> classesToRegister, StringBuilder sb)
        {
            var debuggery = classesToRegister.Where(c => c.debugString is not null);
            if (debuggery.Any())
            {
                sb.AppendLine("const string debuggery2 = @\"");
                foreach (var thing in debuggery)
                {
                    sb.AppendLine(thing.Name + ": " + thing.debugString);
                }
                sb.AppendLine("\";");
            }
        }
    }
}

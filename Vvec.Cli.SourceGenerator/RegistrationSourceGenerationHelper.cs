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

        private static void WriteCommandWithoutArgs(StringBuilder sb, ClassToRegister registration)
        {
            sb.AppendLine(@"
            {
                var command = registeredCommands[typeof(" + registration.Name + @")];
                command.SetHandler(() =>
                {
                    // Need to do IoC resolve in SourceGen rather than reflection...
                    var implementation = (" + registration.Name + ")resolver(typeof(" + registration.Name + @"));
                    implementation.Execute();
                });
            }");
        }

        private static void WriteCommandWithArgs(StringBuilder sb, ClassToRegister registration)
        {
            sb.AppendLine(@"
            {
                var command = registeredCommands[typeof(" + registration.Name + @")];");

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
            var debuggery = classesToRegister.Where(c => c.debuggery is not null);
            if (debuggery.Any())
            {
                sb.AppendLine("const string debuggery = @\"");
                foreach (var thing in debuggery)
                {
                    sb.AppendLine(thing.Name + ": " + thing.debuggery);
                }
                sb.AppendLine("\";");
            }
        }
    }
}

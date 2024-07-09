using System.CommandLine;
using System.Reflection;
using Microsoft.CodeAnalysis; 
using Vvec.Cli.UI;


namespace Vvec.Cli.Arguments.Introspection
{
    /// <summary>
    /// This is a holding spot for my current reflection code for wiring stuff up. This would be a good candidate to convert into source generator code.
    /// For the initial dev work, I'm thinking that reflection might be easier, so I can iterate on stuff quicker. But once it's done, turning it into
    /// a source generator would give quicker code at startup (granted, it's not noticeably slow with reflection at the time of writing) but also be a
    /// useful learning experience for getting one up & running all properlike.
    /// </summary>
    public class CommandFactory
    {
        private Initialiser initialiser;

        public CommandFactory(Initialiser initialiser)
        {
            this.initialiser = initialiser;
        }

        public RootCommand SetUpRootCommand<TSubCommand>(UI.IConsole verbose) where TSubCommand : ISubCommandBase
        {
            var command = new RootCommand(TSubCommand.Description);
            SetUpCommand<TSubCommand>(command, verbose);
            return command;
        }

        public Command SetUpSubCommand<TSubCommand>(UI.IConsole verbose) where TSubCommand : ISubCommandBase
        {
            var command = new Command(TSubCommand.Name, TSubCommand.Description);
            SetUpCommand<TSubCommand>(command, verbose);
            return command;
        }

        private void SetUpCommand<TSubCommand>(Command command, UI.IConsole verbose) where TSubCommand : ISubCommandBase
        {
            var interfaces = typeof(TSubCommand).GetInterfaces();
            var constructedGenericInterface = interfaces.FirstOrDefault(i => i.IsConstructedGenericType);

            if (interfaces.Contains(typeof(ISubCommand)))
                command.SetHandler(() => ((ISubCommand)initialiser.Resolve<TSubCommand>()).Execute());
            else if (interfaces.Contains(typeof(ISubCommandAsync)))
                command.SetHandler(() => ((ISubCommandAsync)initialiser.Resolve<TSubCommand>()).Execute().Wait());
            else if (constructedGenericInterface.GetGenericTypeDefinition() == typeof(ISubCommand<>))
            {
                var args = constructedGenericInterface.GenericTypeArguments.First();
                var fields = new List<(Symbol symbol, PropertyInfo prop)>();
                verbose.WriteLine("Found ", fields.Count.InYellow(), " symbols for ", typeof(TSubCommand).Name.InYellow(), " to load into a ", args.Name.InYellow(), ":");
                foreach (var field in fields)
                {
                    verbose.WriteLine($"  {field.symbol.Name}, {field.prop.Name}, {field.prop.PropertyType}");
                }
                verbose.WriteLine("Now I just need to codegen a function to take all of them as args, and load em into the type, then hook that up to the subcommand. Simples".InDarkRed());
            }
        }
    }
}

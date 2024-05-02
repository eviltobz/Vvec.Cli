//using System.CommandLine;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;
public class VeryLongCommand : ISubCommand<Thingy>
{
    private readonly IConsole cons;

    public VeryLongCommand(IConsole cons)
    {
        this.cons = cons;
    }

    //public static IEnumerable<Symbol> Modifiers => new Symbol[] { new Argument<string>("X", "a string value"), new Argument<int>("Y", "an int value") };

    public static string Name => "very_long_command_with_a_bunch_of_some_more_just_random_blather_to_make_me_long";

    public static string Description => "Hacky method for dev testing commands with a really long name and description for testing word-wrapping functionality...";

    public void Execute(Thingy args)
    {
        cons.WriteLine("Executing a Thingy. StringArg = \"", args.StringArg.InYellow(), "\", BoolArg = \"", args.BoolArg.InYellow(), "\", IntArg = \"", args.ChoiceArg.InYellow(), "\"");
    }
}


//using System.CommandLine;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class FurtleArgsCommand : ISubCommand<Thingy>
{
    private readonly IConsole cons;

    public FurtleArgsCommand(IConsole cons)
    {
        this.cons = cons;
    }

    //public static IEnumerable<Symbol> Modifiers => new Symbol[] { new Argument<string>("X", "a string value"), new Argument<int>("Y", "an int value") };

    public static string Name => "furtleargs";

    public static string Description => "Hacky method for dev testing commands with arguments";

    public void Execute(Thingy args)
    {
        cons.WriteLine("Executing a Thingy. StringArg = \"", args.StringArg.InYellow(), "\", BoolArg = \"", args.BoolArg.InYellow(), "\", IntArg = \"", args.IntArg.InYellow(), "\"");
    }
}

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
        cons.WriteLine("Executing a Thingy. StringArg = \"", args.StringArg.InYellow(), "\", BoolArg = \"", args.BoolArg.InYellow(), "\", IntArg = \"", args.IntArg.InYellow(), "\"");
    }
}

public class Thingy
{
    [Arg("stringy", "a string argument")]
    public string StringArg { get; init; }
    [Arg("booly", "a bool argument")]
    public bool BoolArg { get; init; }
    [Arg("inty", "an int argument")]
    public int IntArg { get; init; }

    //[Argx<string>("ticket", "a number to parse into a jira ticket reference.", ""x => "")]
    //[Argx<int>("ticket", "a number to parse into a jira ticket reference.", null)] //ArgumentParser.Ticket)]
    public string TicketArg { get; init; }
}


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

    public static string Name => "args";

    public static string Description => "Hacky method for dev testing commands with arguments";

    public void Execute(Thingy args)
    {
        cons.WriteLine("Executing a Thingy. StringArg = \"", args.StringArg.InYellow(), "\", BoolArg = \"", args.BoolArg.InYellow(), "\", IntArg = \"", args.ChoiceArg.InYellow(), "\"");
    }
}
public class Thingy
{
    public enum Choices
    {
        This,
        That,
        TheOther
    }

    [Arg("stringy", "a string argument")]
    public string StringArg { get; init; }
    [Arg("booly", "a bool argument")]
    public bool BoolArg { get; init; }
    [Arg("choice", "a choices enum argument")]
    public Choices ChoiceArg { get; init; }

    //[Argx<string>("ticket", "a number to parse into a jira ticket reference.", ""x => "")]
    //[Argx<int>("ticket", "a number to parse into a jira ticket reference.", null)] //ArgumentParser.Ticket)]
    public string TicketArg { get; init; }
}


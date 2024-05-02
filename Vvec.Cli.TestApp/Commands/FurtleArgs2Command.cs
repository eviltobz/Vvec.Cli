//using System.CommandLine;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class FurtleArgs2Command : ISubCommand
{
    private readonly IConsole cons;
    public enum Choices
    {
        This,
        That,
        TheOther
    }

    [Arg("stringy", "a string argument")]
    public string StringArg { get; init; }
    //[Arg("booly", "a bool argument")]
    [Opt('b', "booly", "a bool argument")]
    public bool BoolArg { get; init; }
    [Arg("choice", "a choices enum argument")]
    public Choices ChoiceArg { get; init; }

    public FurtleArgs2Command(IConsole cons)
    {
        this.cons = cons;
    }

    //public static IEnumerable<Symbol> Modifiers => new Symbol[] { new Argument<string>("X", "a string value"), new Argument<int>("Y", "an int value") };

    public static string Name => "args2";

    public static string Description => "Hacky method for dev testing commands with arguments";

    public void Execute()
    {
        cons.WriteLine("Executing with args.. StringArg = \"", StringArg.InYellow(),
            "\", BoolArg = \"", BoolArg.InYellow(),
            "\", IntArg = \"", ChoiceArg.InYellow(), "\"");
    }
}


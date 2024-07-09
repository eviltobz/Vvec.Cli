using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class FurtleArgsCommand : ISubCommand
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
    [Opt('b', "booly", "a bool option")]
    public bool BoolArg { get; init; }
    [Arg("choice", "a choices enum argument")]
    public Choices ChoiceArg { get; init; }

    public static string Name => "args";

    public static string Description => "Hacky method for dev testing commands with arguments";

    public FurtleArgsCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public void Execute()
    {
        cons.WriteLine("Executing with args.. StringArg = \"", StringArg.InYellow(),
            "\", BoolArg = \"", BoolArg.InYellow(),
            "\", IntArg = \"", ChoiceArg.InYellow(), "\"");
    }
}


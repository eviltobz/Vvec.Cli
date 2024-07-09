using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class VeryLongCommand : ISubCommand
{
    private readonly IConsole cons;

    public VeryLongCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public static string Name => "very_long_command_with_a_bunch_of_some_more_just_random_blather_to_make_me_long";

    public static string Description => "Hacky method for dev testing commands with a really long name and description for testing word-wrapping functionality...";

    public void Execute()
    {
        cons.WriteLine("Executing a very long command...");
    }
}

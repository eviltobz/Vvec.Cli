using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

//public class ParentCommand(IConsole cons) : ISubCommandParent
public class ParentCommand() : ISubCommandParent
{
    public static string Name => "parent";

    public static string Description => "Nested SubCommands.";

    //public void Execute()
    //{
    //    cons.WriteLine("Parent command.");
    //}
}

public class Gen1Command(IConsole cons) : ISubCommand
{
    public static string Name => "gen1";

    public static string Description => "First gen subcommand";

    public void Execute()
    {
        cons.WriteLine("Gen1 command.");
    }
}

public class Gen2Command(IConsole cons) : ISubCommand
{
    public static string Name => "gen2";

    public static string Description => "Second gen subcommand";

    public void Execute()
    {
        cons.WriteLine("Gen2 command.");
    }
}

public class OtherCommand(IConsole cons) : ISubCommandAsync
{
    public static string Name => "other";

    public static string Description => "Another first gen subcommand";

    //public void Execute()
    //{
    //    cons.WriteLine("Other command in non-Async Flavour.");
    //}

    public Task Execute()
    {
        cons.WriteLine("Other command in Async Flavour.");
        return Task.CompletedTask;
    }
}
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class ConfigCheckCommand : ISubCommand
{
    private readonly Config config;
    private readonly IConsole cons;

    public static string Name => "ConfigCheck";

    public static string Description => "Take the config as a dependency & display it";

    public ConfigCheckCommand(IConsole cons, Config config)
    {
        this.cons = cons;
        this.config = config;
    }

    public void Execute()
    {
        cons.WriteLine("Some config is StringValue:", FG.Yellow, config.StringValue);
    }
}


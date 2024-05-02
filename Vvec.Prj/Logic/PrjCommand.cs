using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class PrjCommand(IConsole cons, Config config, IIO io, IOpenDirect openDirect, OpenFromList openFromList) : ISubCommand
{
    public static string Name => "Default";

    public static string Description => "Choose a directory in the configured project folder, or 'cd' straight to specified one";

    [Arg("Shortcut", "Match to a configured shortcut or folder in the project root to open directly")]
    public string Shortcut { get; init; }

    public void Execute()
    {
        if (config.ProjectRoot is null)
        {
            cons.WriteLine("No ProjectRoot has been configured.".InRed());
            return;
        }

        var root = io.GetSubfolders(config.ProjectRoot);

        string path = null;
        if (Shortcut is not null)
            path = openDirect.FindPath(Shortcut, root);

        if (path is null)
            path = openFromList.ChoosePath(root);

        io.ReturnPathToShell(path);
    }
}


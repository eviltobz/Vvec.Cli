using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class PrjCommand(IConsole cons, Config config, IIO io, IOpenDirect openDirect, OpenFromList openFromList) : ISubCommand
{
    public static string Name => "Default";

    public static string Description => "Choose a directory in the configured project folder, or 'cd' straight to specified one";

    [Arg("Shortcut", "Match to a configured shortcut or folder in the project root to open directly")]
    public string Shortcut { get; init; }

    // I might want to make git stuff an option in the future, if it gets slow, but for now
    // it's nice & snappy, so let's just always do it...
    //[Opt('g', "git", "Show git status")]
    //public bool Git { get; init; }

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
            path = openFromList.ChoosePath(root, true);

        io.ReturnPathToShell(path);
    }
}

//public class GitUpdateCommand(IConsole cons, Config config, IIO io, IOpenDirect openDirect, OpenFromList openFromList) : ISubCommandAsync
//{
//    public static string Name => "update";

//    public static string Description => "Update any git repos on master without pending changes";

//    public Task Execute()
//    {
//        throw new NotImplementedException();
//    }
//}

using Vvec.Cli.Arguments;
using Vvec.Cli.Config;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class PrjCommand(IConsole cons, Config config, IIO io, IOpenDirect openDirect, OpenFromList openFromList, PartialMatcher matcher) : ISubCommand
{
    public static string Name => "Default";

    public static string Description => "Choose a directory in the configured project folder, or 'cd' straight to specified one";

    [Arg("Shortcut", "Match to a configured shortcut or folder in the project root to open directly")]
    public string Shortcut { get; init; }

    [Opt('r', "root", "Specify an alternative root project folder")]
    public string OverrideRootFolder { get; init; }

    // I might want to make git stuff an option in the future, if it gets slow, but for now
    // it's nice & snappy, so let's just always do it...
    //[Opt('g', "git", "Show git status")]
    //public bool Git { get; init; }

    public void Execute()
    {
        if (config.ProjectRoot is null)
        {
            cons.WriteLine(FG.Red, "No ProjectRoot has been configured.");
            return;
        }

        string[] projectFolders;
        string actualRootFolder = config.ProjectRoot;
        if (OverrideRootFolder is not null)
        {
            var (rootPath, multiple) = matcher.Find("alternative root folder", OverrideRootFolder, config.ProjectRoots);
            //if (config.ProjectRoots.TryGetValue(OverrideRootFolder, out FolderPath projectFolder))
            if (rootPath is not null)
            {
                //projectFolders = io.GetSubfolders(rootPath);
                actualRootFolder = rootPath.Value;
            }
            else if (multiple)
            {
                return;
            }
            else
            {
                cons.WriteLine(FG.Red, "Overriden root folder '", FG.DarkRed, OverrideRootFolder, FG.Red, "' not found.", FG.Default);
                cons.Write("  Valid override options are: ");
                var names = config.ProjectRoots.Keys;
                var i = 0;
                for (i = 0; i < names.Count - 1; i++)
                {
                    cons.Write(FG.Yellow, names.ElementAt(i), FG.Default, ", ");
                }
                cons.Write(FG.Yellow, names.ElementAt(i), FG.Default, ".");
                return;
            }
        }

        projectFolders = io.GetSubfolders(actualRootFolder);

        string path = null;
        if (Shortcut is not null)
            //path = openDirect.FindPath(Shortcut, projectFolders, OverrideRootFolder is not null);
            path = openDirect.FindPath(Shortcut, projectFolders, actualRootFolder);

        if (path is null)
            //path = openFromList.ChoosePath(projectFolders, actualRootFolder, OverrideRootFolder is not null);// true);
            path = openFromList.ChoosePath(projectFolders, actualRootFolder);

        io.ReturnPathToShell(path);
    }
}

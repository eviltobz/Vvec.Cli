using Vvec.Cli.Config;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class OpenDirect(IConsole cons, Config config, PartialMatcher matcher) : IOpenDirect
{
    //public string? FindPath(string hint, string[] projectFolders, bool overrideRootFolder)
    //{
    //    var normalisedHint = hint.ToUpper();
    //    if (!overrideRootFolder)
    //    {
    //        var (shortcutPath, multipleMatchingShortcuts) = matcher.Find<FolderPath>("shortcut", hint, config.Shortcuts);
    //        if (shortcutPath is not null)
    //            return shortcutPath;
    //        if (multipleMatchingShortcuts)
    //            return null;
    //    }

    //    var folders = projectFolders.ToDictionary(x => x.Substring(config.ProjectRoot!.Value.Length), x => x);
    //    var (folderPath, multipleMatchingFolders) = matcher.Find<string>("path", hint, folders);
    //    if (folderPath is not null)
    //        return folderPath;
    //    if (multipleMatchingFolders)
    //        return null;

    //    cons.WriteLine(FG.Red, "No match for '", FG.Default, hint, FG.Red, "'");
    //    return null;
    //}

    public string? FindPath(string hint, string[] projectFolders, string rootFolder)
    {
        if (hint == ".")
            return rootFolder;

        var normalisedHint = hint.ToUpper();
        if (rootFolder.ToUpper() == config.ProjectRoot!.Value.ToUpper() )
        {
            var (shortcutPath, multipleMatchingShortcuts) = matcher.Find<FolderPath>("shortcut", hint, config.Shortcuts);
            if (shortcutPath is not null)
                return shortcutPath;
            if (multipleMatchingShortcuts)
                return null;
        }

        var folders = projectFolders.ToDictionary(x => x.Substring(config.ProjectRoot!.Value.Length), x => x);
        var (folderPath, multipleMatchingFolders) = matcher.Find<string>("path", hint, folders);
        if (folderPath is not null)
            return folderPath;
        if (multipleMatchingFolders)
            return null;

        cons.WriteLine(FG.Red, "No match for '", FG.Default, hint, FG.Red, "'");
        return null;
    }
}

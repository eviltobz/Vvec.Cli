using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class OpenFromList(IConsole cons, Config config, IOpenDirect openDirect)
{
    public string? ChoosePath(string[] projectFolders, bool git)
    {
        cons.WriteLine("Shortcuts".InGreen());
        foreach (var shortcut in config.Shortcuts)
        {
            cons.WriteLine("  [", shortcut.Key.InYellow(), "] ", shortcut.Value.Value);
        }

        cons.WriteLine($"Project Folders in {config.ProjectRoot}".InGreen());
        for (int i = 0; i < projectFolders.Length; i++)
        {
            var directory = projectFolders[i];
            cons.Write("  [", i.InYellow(), "] ", directory.Substring(config.ProjectRoot!.Value.Length));
            if (git)
            {
                var repo = new Git(directory);
                if (repo.IsValid)
                    cons.Write(" (", repo.HasUncommittedChanges ? repo.CurrentBranch.InRed() : repo.CurrentBranch.InGreen(), ")");
                else
                    cons.Write(" (", "N/A".InDarkGrey(), ")");
            }
            cons.WriteLine();
        }

        var selection = cons.StartPrompt("Select folder").GetFreeText();

        if (string.IsNullOrWhiteSpace(selection))
            return null;

        var isIndex = int.TryParse(selection, out int index);
        if (isIndex && index >= 0 && index < projectFolders.Length)
        {
            return projectFolders[index];
        }

        return openDirect.FindPath(selection, projectFolders);
    }
}

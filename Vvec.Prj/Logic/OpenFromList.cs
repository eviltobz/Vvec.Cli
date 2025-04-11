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

        var tasks = new Task[projectFolders.Length];
        for (int i = 0; i < projectFolders.Length; i++)
        {
            var directory = projectFolders[i];
            var line = cons.StartAppendable("  [", i.InYellow(), "] ", directory.Substring(config.ProjectRoot!.Value.Length));
            var task = new Task(() =>
            {
                if (git)
                {
                    line.StartSpinner();
                    var repo = new Git(directory);
                    if (repo.IsValid)
                        line.Write(" (", repo.HasUncommittedChanges ? repo.CurrentBranch.InRed() : repo.CurrentBranch.InGreen(), ")");
                    else
                        line.Write(" (", "N/A".InDarkGrey(), ")");
                }
            });
            tasks[i] = task;
            task.Start();
        }
        Task.WhenAll(tasks).Wait();

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

using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class OpenFromList(IConsole cons, Config config, IOpenDirect openDirect)
{
    public string? ChoosePath(string[] projectFolders, string rootFolder)
    //public string? ChoosePath(string[] projectFolders, string rootFolder, bool overrideRootFolder)
    {
        //if (!overrideRootFolder)
        if (rootFolder.ToUpper() == config.ProjectRoot!.Value.ToUpper())
        {
            cons.WriteLine(FG.Green, "Shortcuts");
            foreach (var shortcut in config.Shortcuts)
            {
                cons.WriteLine("  [", FG.Yellow, shortcut.Key, FG.Default, "] ", shortcut.Value.Value);
            }
        }

        cons.WriteLine(FG.Green, "Project Folders in ", FG.DarkGreen, rootFolder, FG.Default);

        cons.WriteLine(
            "  [", FG.Yellow, ".", FG.Default, "] ",
            (projectFolders.Length > 10 ? " " : ""),
            "\\");

        var tasks = new Task[projectFolders.Length];
        for (int i = 0; i < projectFolders.Length; i++)
        {
            var directory = projectFolders[i];
            var line = cons.StartAppendable(
                "  [", FG.Yellow, i, FG.Default, "] ",
                (projectFolders.Length > 10 && i < 10 ? " " : ""),
                directory.Substring(rootFolder.Length));
            var task = new Task(() =>
            {
                var repo = new Git(directory);
                if (repo.IsValid)
                {
                    //line.Write(" (", repo.HasUncommittedChanges ? FG.Red : FG.Green, repo.CurrentBranch);
                    //line.Write(repo.Tracking.ToConsole());
                    //line.Write([" ", .. repo.GetChanges().ToConsole()]);
                    //line.Write(")");

                    line.Write([
                        " (",
                        repo.HasUncommittedChanges ? FG.Red : FG.Green,
                        repo.CurrentBranch,
                        ..repo.Tracking.ToConsole(),
                        " ",
                        ..repo.GetChanges().ToConsole(),
                        ")"]);
                }
                else
                    line.Write(" (", FG.DarkGrey, "N/A", FG.Default, ")");
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

        //return openDirect.FindPath(selection, projectFolders, overrideRootFolder);
        return openDirect.FindPath(selection, projectFolders, rootFolder);
    }
}

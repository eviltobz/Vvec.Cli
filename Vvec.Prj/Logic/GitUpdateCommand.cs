using LibGit2Sharp;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

public class GitCommand() : ISubCommandParent
{
    public static string Name => "git";

    public static string Description => "Commands for managing the repos.";
}

public class GitRevertCommand(IConsole cons, Config config, IIO io, GitUpdateCommand updater) : ISubCommandAsync
{
    public static string Name => "revert";

    public static string Description => "Revert valid branches to 'main' and update repos";

    public async Task Execute()
    {
        string[] projectFolders = io.GetSubfolders(config.ProjectRoot);

        var title = cons.StartAppendable($"Getting git repos in {config.ProjectRoot}").StartSpinner();
        var repos = projectFolders.Select(f => new Git(f)).Where(r => r.IsValid);
        title.Write($". {repos.Count(r => r.IsValid)} repos found.");

        var revertable = repos.Where(r => r.IsRevertable()).ToArray();

        if (revertable.Any())
        {
            cons.WriteLine(FG.Yellow, "Reverting ", FG.DarkYellow, revertable.Length, FG.Yellow, " branches to '", FG.DarkYellow, "main", FG.Yellow, "'");
            foreach (var repo in revertable)
            {
                var appender = cons.StartAppendable("  ", repo.Name, " ", FG.DarkCyan, repo.CurrentBranch).StartSpinner();
                repo.CheckoutMain();
                appender.Write(" > ", FG.DarkGreen, "main");
            }

            await updater.Execute();
        }
        else
        {
            cons.WriteLine(FG.DarkGreen, "No revertable repos were found.");
        }
    }
}

public class GitUpdateCommand(IConsole cons, Config config, IIO io) : ISubCommandAsync
{
    private static readonly object locker = new object();

    public static string Name => "update";

    public static string Description => "Update any git repos on master without pending changes";

    public async Task Execute()
    {
        string[] projectFolders = io.GetSubfolders(config.ProjectRoot);

        var title = cons.StartAppendable($"Getting git repos in {config.ProjectRoot}").StartSpinner();
        var repos = projectFolders.Select(f => new Git(f)).Where(r => r.IsValid);
        title.Write($". {repos.Count(r => r.IsValid)} repos found.");

        var uncommitted = repos.Where(r => r.HasUncommittedChanges).ToArray();
        if (uncommitted.Any())
        {
            //cons.WriteLine($"Not updating ".InYellow(), uncommitted.Length.InDarkYellow(), " repos with uncommitted changes.".InYellow());
            title = cons
                //.StartAppendable($"Not updating ".InYellow(), uncommitted.Length.InDarkYellow(), " repos with uncommitted changes.".InYellow())
                .StartAppendable(FG.Yellow, "Not updating ", FG.DarkYellow, uncommitted.Length, FG.Yellow, " repos with uncommitted changes.")
                .StartSpinner();

            foreach (var repo in uncommitted)
            {
                //var changes = repo.GetChanges();
                cons.WriteLine(["  ", FG.DarkGrey, GetFolderName(repo.Path),
                    FG.Default, " (",
                    repo.CurrentBranch == "main" ? FG.Green : FG.Red,
                    repo.CurrentBranch, // "main".InGreen() : repo.CurrentBranch.InRed(),
                    " ", ..repo.GetChanges().ToConsole(),
                    FG.Default, ")"]);
            }

            title.Write();
        }

        var onBranch = repos.Where(r => !r.HasUncommittedChanges && r.CurrentBranch != "main").ToArray();
        var notRevertable = onBranch.Where(r => !r.IsRevertable()).ToArray();
        var revertable = onBranch.Where(r => r.IsRevertable()).ToArray();
        if (notRevertable.Any())
        {
            title = cons
                .StartAppendable(FG.Yellow, "Not updating ", FG.DarkYellow, notRevertable.Length, FG.Red, " non-revertable ", FG.Yellow,  "repos on branches other than '", FG.DarkYellow, "main", FG.Yellow, "'.")
                .StartSpinner();

            foreach (var repo in notRevertable)
            {
                cons.WriteLine(["  ", FG.DarkGrey, GetFolderName(repo.Path),
                    FG.Default, " (", FG.Red, repo.CurrentBranch,
                    repo.Tracking.IsTracking ? "" : $" {FG.DarkRed}NOT TRACKING",
                    " ", ..repo.GetChanges().ToConsole(),
                    FG.Default, ")"]);
            }

            title.Write();
        }
        if (revertable.Any())
        {
            title = cons
                .StartAppendable(FG.Yellow, "Not updating ", FG.DarkYellow, revertable.Length, FG.Green, " revertable ", FG.Yellow, "repos on branches other than '", FG.DarkYellow, "main", FG.Yellow, "'.")
                .StartSpinner();

            foreach (var repo in revertable)
            {
                cons.WriteLine(["  ", FG.DarkGrey, GetFolderName(repo.Path),
                    FG.Default, " (", FG.Red, repo.CurrentBranch,
                    repo.Tracking.IsTracking ? "" : $" {FG.DarkRed}NOT TRACKING",
                    " ", ..repo.GetChanges().ToConsole(),
                    FG.Default, ")"]);
            }

            title.Write();
        }

        var updateable = repos.Where(r => !r.HasUncommittedChanges && r.CurrentBranch == "main").ToArray();
        if (updateable.Any())
        {
            //cons.WriteLine($"Updating ".InYellow(), updateable.Length.InDarkGreen(), " repos.".InYellow());
            title = cons
                //.StartAppendable($"Updating ".InYellow(), updateable.Length.InDarkGreen(), " repos.".InYellow())
                .StartAppendable(FG.Yellow, "Updating ", FG.DarkGreen, updateable.Length, FG.Yellow, " repos.")
                .StartSpinner();

            var tasks = new List<Task>();
            foreach (var repo in updateable)
            {
                var line = cons.StartAppendable("  ", GetFolderName(repo.Path), " ");
                var status = line.StartStatus("Waiting to update").WithEllipsis();
                var task = new Task(() => TryUpdate(repo, status));
                tasks.Add(task);
                //task.Start();
            }
            //await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                task.Start();
                await task;
            }

            title.Write();
        }
    }

    string GetFolderName(string fullPath) =>
        fullPath.Substring(config.ProjectRoot!.Value.Length);

    private void TryUpdate(Git repo, IConsole.IStatus status)
    {
        if (repo.HasUncommittedChanges)
        {
            status.Finish(FG.Red, "Repo has uncommitted changes, so not updating.");
            return;
        }

        if (repo.CurrentBranch != "main")
        {
            status.Finish(FG.DarkGrey, "Not on main, so not updating.");
            return;
        }

        lock (locker)
        {
            status.Update("Updating repo").WithEllipsis();
            try
            {
                var result = repo.Pull(progress => status.Update(progress));
                status.Finish(result.ToConsole());
            }
            catch (Exception ex)
            {
                status.Finish($"Exceptionful: {ex.Message}");
            }
        }
    }

}

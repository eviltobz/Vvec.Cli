using System.CommandLine;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Vvec.Cli.UI;

namespace Vvec.Prj.Logic;

/// <summary>
/// This uses LibGit2Sharp for the git interactions. Documentation can be found at:
/// https://github.com/libgit2/libgit2sharp/wiki
/// </summary>
public class Git
{
    private readonly Repository repo;
    public readonly bool IsValid;
    public readonly string Path;
    public readonly string Name;

    public Git(string path)
    {
        Path = path;
        if (Repository.IsValid(path))
        {
            repo = new Repository(path);
            StatusOptions opts = new() { };
            var meh = repo.RetrieveStatus(opts);
            IsValid = true;
            var index = Path.LastIndexOf("\\") + 1;
            Name = Path.Substring(index);
        }
        else
            IsValid = false;
    }

    public string CurrentBranch { get => repo.Head.FriendlyName; }

    public bool HasUncommittedChanges { get => repo.RetrieveStatus().IsDirty; }

    public GitChanges GetChanges()
    {
        var status = repo.RetrieveStatus();
        return new(
            status.Added.Count(),
            status.Staged.Count(),
            status.Removed.Count(),
            status.Untracked.Count(),
            status.Modified.Count(),
            status.Missing.Count());
    }

    public GitTracking Tracking
    {
        get => new GitTracking(
            repo.Head.IsTracking,
            repo.Head.TrackingDetails.AheadBy ?? 0,
            repo.Head.TrackingDetails.BehindBy ?? 0);
    }

    public string Revertability()
    {
        var track = Tracking;
        //var revertable =
        //    !HasUncommittedChanges
        //    && track.IsTracking && track.Ahead == 0 && track.Behind == 0
        //    && CurrentBranch != "main";

        var revertable = IsRevertable();

        var retval = $"{FG.DarkCyan}<RV " +
            $"UC:{PrintRevertability(!HasUncommittedChanges, HasUncommittedChanges)} " +
            $"T:{PrintRevertability(track.IsTracking, track.IsTracking)}," +
                $"{PrintRevertability(track.Ahead == 0, track.Ahead)}," +
                $"{PrintRevertability(track.Behind == 0, track.Behind)} " +
            $"B:{PrintRevertability(CurrentBranch != "main", CurrentBranch)} " +
            $"{FG.Yellow}R:{PrintRevertability(revertable, revertable)}" +
            $">{FG.Default}";

        return retval;
    }

    public bool IsRevertable()
    {
        var track = Tracking;
        return !HasUncommittedChanges
            && track.IsTracking && track.Ahead == 0 && track.Behind == 0
            && CurrentBranch != "main";

    }

    private string PrintRevertability(bool condition, object state)
    {
        if (condition)
            return $"{FG.Green}{state}{FG.DarkCyan}";
        else
            return $"{FG.Red}{state}{FG.DarkCyan}";
    }

    public void CheckoutMain()
    {
        if (!IsRevertable())
        {
            throw new InvalidOperationException($"Cannot revert {Path} to main.");
        }

        var branch = repo.Branches.Single(b => b.FriendlyName == "main");
        Commands.Checkout(repo, branch);
    }

    public MergeResult Pull(/*Cli.UI.IConsole.IStatus TEMPstatus,*/ Action<string> update)
    {
        if (CurrentBranch != "main" || HasUncommittedChanges)
            throw new Exception("Nah mate!");

        // Credential information to fetch
        PullOptions options = new PullOptions();
        options.FetchOptions = new FetchOptions();
        //TEMPstatus.WriteLine($"Starting pull for {repo.Info.Path}");
        var fetch = options.FetchOptions;
        fetch.OnProgress = new ProgressHandler(status =>
        {
            // Progress can be multiple (and potentially incomplete?) lines
            // This currently gets the last line (given the types of message I've seen)
            // but it may need further tweaking if we did ever get partial lines...
            var lines = status.Split('\r');
            var actualLines = lines.Where(l => l.Length > 0);
            var lastMessage = actualLines.Last().Replace("\n", "");
            //TEMPstatus.Update(actualLines.Last().Replace("\n", ""));
            update(lastMessage);
            return true;
        });
        fetch.OnTransferProgress = new TransferProgressHandler(progress =>
        {
            //TEMPstatus.Update($"Transferring " + $"{progress.ReceivedObjects}/{progress.TotalObjects} ({progress.ReceivedBytes}b) ");
            var message = $"Transferring " + $"{progress.ReceivedObjects}/{progress.TotalObjects} ({progress.ReceivedBytes}B ";
            update(message);
            return true;
        });
        //fetch.OnUpdateTips = new UpdateTipsHandler((message, oldId , newId) =>
        //{ TEMPcons.WriteLine($"UpdateTips: {message}, {oldId}, {newId}"); return true; });
        //fetch.RepositoryOperationStarting = new RepositoryOperationStarting(ctx =>
        //{ TEMPcons.WriteLine($"RepoOpStart:{ctx.RepositoryPath},{ctx.ParentRepositoryPath},{ctx.SubmoduleName}"); return true; });
        //fetch.RepositoryOperationCompleted = new RepositoryOperationCompleted(ctx =>
        //    TEMPcons.WriteLine($"RepoOpComplete:{ctx.RepositoryPath},{ctx.ParentRepositoryPath},{ctx.SubmoduleName}"));

        options.FetchOptions.CredentialsProvider = new CredentialsHandler(
            (url, usernameFromUrl, types) =>
                new UsernamePasswordCredentials()
                {
                    /*
                     *
                     * DO NOT COMMIT THIS YOU FOO!!!!
                     *
                     */
                    //Password = "password here..."
                });
        //new DefaultCredentials());

        // User information to create a merge commit
        var signature = new Signature(
            "TobyCarterAngstrom",
            "tobias.carter@angstromsports.com",
            DateTimeOffset.Now);

        // Pull
        var result = Commands.Pull(repo, signature, options);


        //TEMPstatus.WriteLine($"Finished pull for {repo.Info.Path}").WriteLine();
        return result;
    }
}

public record GitChanges(int Added, int Staged, int Removed, int Untracked, int Modified, int Missing)
{
    public int Total { get => Added + Staged + Removed + Untracked + Modified + Missing; }
}

public record GitTracking(bool IsTracking, int Ahead, int Behind)
{ }

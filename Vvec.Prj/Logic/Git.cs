using LibGit2Sharp;

namespace Vvec.Prj.Logic;

public class Git
{
    private readonly Repository repo;
    public readonly bool IsValid;

    public Git(string path)
    {
        if (Repository.IsValid(path))
        {
            repo = new Repository(path);
            StatusOptions opts = new() { };
            var meh = repo.RetrieveStatus(opts);
            IsValid = true;
        }
        else
            IsValid = false;
    }

    public string CurrentBranch { get => repo.Head.FriendlyName; }

    public bool HasUncommittedChanges { get => repo.RetrieveStatus().IsDirty; }

}

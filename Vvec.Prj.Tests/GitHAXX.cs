using Vvec.Prj.Logic;

namespace Vvec.Prj.Tests;

public class GitHAXX
{
    [Test]
    public void PokeRepo()
    {
        var git = new Git(@"c:/dev/eviltobz/vvec.cli");
        var bob = git.CurrentBranch + $", {git.HasUncommittedChanges}";
        Assert.Fail(bob);
    }

}

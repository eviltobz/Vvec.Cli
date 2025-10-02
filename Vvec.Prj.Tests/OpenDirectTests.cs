using Vvec.Cli.UI;
using Vvec.Prj.Logic;

namespace Vvec.Prj.Tests;

public class OpenDirectTests
{
    private OpenDirect sut;
    private string[] projectFolders;
    private TestConsole testConsole;

    [SetUp]
    public void Setup()
    {
        var config = new Logic.Config()
        {
            ProjectRoot = new Cli.Config.FolderPath("projectRoot"),
            Shortcuts = new()
            {
                { "VeryDistinct", new Cli.Config.FolderPath(@"config\PathA\") },
                { "Path1", new Cli.Config.FolderPath(@"config\PathB\") },
                { "Path2", new Cli.Config.FolderPath(@"config\PathC\") },
                { "Match", new Cli.Config.FolderPath(@"config\PathD\") },
                { "MatchingStartWithExtra", new Cli.Config.FolderPath(@"config\PathE\") },
                { "InBoth", new Cli.Config.FolderPath(@"config\InBoth\") },
            }
        };

        projectFolders = new string[]
            {
                @"projectRoot\Path1",
                @"projectRoot\Path3",
                @"projectRoot\Folder1",
                @"projectRoot\Folder2",
                @"projectRoot\SomeInterestingProject",
                @"projectRoot\InBoth",
            };

        testConsole = VConsole.CreateTestConsole(Assert.Fail);
        var matcher = new PartialMatcher(testConsole);

        sut = new OpenDirect(testConsole, config, matcher);
    }

    // From config
    [TestCase("VeryDistinct", @"config\PathA\")]
    [TestCase("path1", @"config\PathB\")]
    [TestCase("path2", @"config\PathC\")]
    [TestCase("MATCH", @"config\PathD\")]
    [TestCase("MATchingstARTwithEXTRA", @"config\PathE\")]
    [TestCase("inBoth", @"config\InBoth\")]
    // From project root
    [TestCase("path3", @"projectRoot\Path3")]
    [TestCase("FOLDER1", @"projectRoot\Folder1")]
    [TestCase("fOlDeR2", @"projectRoot\Folder2")]
    [TestCase("SomeInterestingProject", @"projectRoot\SomeInterestingProject")]
    // Project root checks exclude the root path
    [TestCase(@"projectRoot\path3", null)]
    [TestCase("not a match", null)]
    public void ReturnsForFullMatchCaseInsensitivePrioritisingShortcuts(string shortcut, string? expected)
    {
        var actual = sut.FindPath(shortcut, projectFolders, false);

        Assert.AreEqual(expected, actual);
    }

    // From config
    [TestCase("1", @"config\PathB\")]
    [TestCase("2", @"config\PathC\")]
    [TestCase("v", @"config\PathA\")]
    [TestCase("matchi", @"config\PathE\")]
    [TestCase("both", @"config\InBoth\")]
    // From project root
    [TestCase("3", @"projectRoot\Path3")]
    [TestCase("r2", @"projectRoot\Folder2")]
    [TestCase("some", @"projectRoot\SomeInterestingProject")]
    [TestCase("InT", @"projectRoot\SomeInterestingProject")]
    [TestCase("J", @"projectRoot\SomeInterestingProject")]
    // Project root checks exclude the root path
    [TestCase(@"projectRoot\some", null)]
    public void ReturnsForDistinctPartialMatchCaseInsensitivePrioritisingShortcuts(string shortcut, string? expected)
    {
        var actual = sut.FindPath(shortcut, projectFolders, false);

        Assert.AreEqual(expected, actual);
    }

    // From config
    [TestCase("path", "shortcut", "PathB", "PathC")]
    // From project root
    [TestCase("folder", "path", "Folder1", "Folder2")]
    public void WritesErrorForNonDistictPartialMatchCaseInsensitive(string partial, string type, string match1, string match2)
    {
        var actual = sut.FindPath(partial, projectFolders, false);

        Assert.IsNull(actual);
        testConsole.AssertLineContains(type, partial);
        testConsole.AssertLineContains(match1);
        testConsole.AssertLineContains(match2);
    }

    [TestCase("not a match")]
    [TestCase("banana")]
    public void WritesErrorForNoMatch(string name)
    {
        var actual = sut.FindPath(name, projectFolders, false);

        Assert.IsNull(actual);
        testConsole.AssertLineContains("no match", name);
    }
}

using Moq;
using Vvec.Cli.UI;
using Vvec.Prj.Logic;

namespace Vvec.Prj.Tests;

public class OpenFromListTests
{
    private OpenFromList sut;
    private string[] projectFolders;
    private TestConsole testConsole;
    private Mock<IOpenDirect> mockOpenDirect;

    [SetUp]
    public void Setup()
    {
        var config = new Config()
        {
            ProjectRoot = new Cli.Config.FolderPath("projectRoot"),
            Shortcuts = new()
            {
                { "config1", new Cli.Config.FolderPath(@"somepath\config1\") },
                { "config2", new Cli.Config.FolderPath(@"otherpath\config2\") },
            }
        };

        projectFolders = new string[]
            {
                @"projectRoot\Path1",
                @"projectRoot\Path2",
            };

        testConsole = VConsole.CreateTestConsole(Assert.Fail);

        mockOpenDirect = new Mock<IOpenDirect>();
        sut = new OpenFromList(testConsole, config, mockOpenDirect.Object);
    }

    [Test]
    public void ListsShortcuts()
    {
        testConsole.SetupLinesToRead("blah");
        sut.ChoosePath(projectFolders, false);

        testConsole.AssertLineContains("[config1]", @"somepath\config1");
        testConsole.AssertLineContains("[config2]", @"otherpath\config2");
    }

    [Test]
    public void ListsProjectFolders()
    {
        testConsole.SetupLinesToRead("blah");
        sut.ChoosePath(projectFolders, false);

        testConsole.AssertLineContains("[0]", @"Path1");
        testConsole.AssertLineContains("[1]", @"Path2");
    }

    [TestCase("0")]
    [TestCase("5")]
    [TestCase("10")]
    public void InputAValidNumberToSelectAProjectFolder(string number)
    {
        projectFolders = new[]
        {
            @"projectRoot\path0",
            @"projectRoot\path1",
            @"projectRoot\path2",
            @"projectRoot\path3",
            @"projectRoot\path4",
            @"projectRoot\path5",
            @"projectRoot\path6",
            @"projectRoot\path7",
            @"projectRoot\path8",
            @"projectRoot\path9",
            @"projectRoot\path10",
        };
        testConsole.SetupLinesToRead(number);

        var actual = sut.ChoosePath(projectFolders, false);

        Assert.AreEqual(@"projectRoot\path" + number, actual);
        mockOpenDirect.VerifyNoOtherCalls();
    }

    [TestCase("-1")]
    [TestCase("2")]
    [TestCase("10")]
    [TestCase("some text")]
    public void NotValidIndexFallsBackToOpenDirect(string input)
    {
        const string openDirectResponse = "Open Direct Response";
        mockOpenDirect.Setup(x => x.FindPath(input, projectFolders)).Returns(openDirectResponse);
        testConsole.SetupLinesToRead(input);

        var actual = sut.ChoosePath(projectFolders, false);

        mockOpenDirect.Verify(x => x.FindPath(input, projectFolders), Times.Once());
        Assert.AreEqual(openDirectResponse, actual);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void EmptyInputReturnsNull(string? input)
    {
        testConsole.SetupLinesToRead(input);

        var actual = sut.ChoosePath(projectFolders, false);

        Assert.IsNull(actual);
        mockOpenDirect.VerifyNoOtherCalls();
    }
}

using System.CommandLine.IO;

namespace Vvec.Cli.Arguments.ParserConsole;

/// This is nasty & hacky, but _so far_ it seems to work ok :)
/// Testing Notes:
/// The following are different command lines to test with that should have differences in their output that could show up problems
/// dz
/// dz /?
/// dz start /?
/// dz --version

public class ArgumentParserConsole : System.CommandLine.IConsole
{
    private readonly List<KeyValuePair<string, List<string>>> Groups;
    private List<string> CurrentGroup = new List<string>();
    private readonly StandardWriter standard;
    private readonly ErrorWriter error = new ErrorWriter();

    public ArgumentParserConsole()
    {
        Groups = new List<KeyValuePair<string, List<string>>>();
        standard = new StandardWriter(Groups);
    }

    public IStandardStreamWriter Out => standard;

    public bool IsOutputRedirected => false;

    public IStandardStreamWriter Error => error;

    public bool IsErrorRedirected => false;

    public bool IsInputRedirected => false;


    public void NoGroups()
    {
        CurrentGroup = new List<string>();
        Groups.Add(new("Commands", CurrentGroup));
    }

    public void AddGroup(string name)
    {
        if (!Groups.Any() && CurrentGroup.Any())
        {
            Groups.Add(new("Main Commands", CurrentGroup));
        }
        CurrentGroup = new List<string>();
        Groups.Add(new(name, CurrentGroup));
    }

    public void AddCommand(string name)
    {
        // expect that command names are proceeded by 2 spaces
        CurrentGroup.Add("  " + name);
    }
}

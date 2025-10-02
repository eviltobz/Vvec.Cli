using System.CommandLine.IO;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class ErrorWriter : IStandardStreamWriter
{
    private IConsole console = VConsole.Instance;
    private int writeCount = 0;
    private HashSet<string> errors = new HashSet<string>();

    public void Write(string? value)
    {
        if (value == "Required command was not provided." + Environment.NewLine
            || (value == Environment.NewLine && writeCount == 0)
            || errors.Contains(value))
            return;

        console.Write(FG.DarkRed, value);//.InDarkRed());
        writeCount++;
        errors.Add(value);
    }
}

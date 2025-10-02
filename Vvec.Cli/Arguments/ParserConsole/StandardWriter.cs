using System.CommandLine.IO;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class StandardWriter : IStandardStreamWriter
{
    private readonly CommandWriter commandWriter;
    private readonly IConsole console = VConsole.Instance;

    private int callCount = 0;
    private bool isNewline = true;
    private List<KeyValuePair<string, List<string>>> groups;
    private int groupIndex = 0;
    private bool isOpts = false;
    private bool isUsage = false;
    private int optCount = 0;
    private bool isSubCommand = false;

    public void Write(string? value)
    {
        if (value is null)
            return;

        callCount++;
        var segments = value.Split(".");
        if (callCount == 1 & segments.Length == 3) // Not a perfect check, but _should_ be enough
        {
            console.Write(value);
        }

        else
        {
            if (IsProcessingCommands(value))
                commandWriter.Write(value);
            else
                WriteSegment(value);
        }
    }

    public StandardWriter(List<KeyValuePair<string, List<string>>> groups)
    {
        this.groups = groups;
        commandWriter = new CommandWriter(groups);
    }

    // This is getting nasty, it might be worth splitting out more sub-writers, at least for options
    private void WriteSegment(string value)
    {
        if (isNewline)
        {
            if (value.StartsWith(" "))
            {
                if (isOpts)
                {
                    if(optCount == 0 && value != "  --version")
                        isSubCommand = true;

                    if (isSubCommand && optCount > 0 && value == "  -?, -h, --help")
                    {
                        console.WriteLine("  --------------");
                    }
                    optCount++;
                }

                console.Write(FG.DarkYellow, value);//.InDarkYellow());
            }
            else
            {
                if (isOpts && value == "\r")
                {
                    console
                        .WriteLine(FG.DarkYellow, "  --dc       ", "     ", FG.Red, "Use default console for this help page")
                        .Write(FG.DarkYellow, "  --verbose  ", "     ", FG.Cyan, "Include extra debug output");
                        //.WriteLine("  --dc       ".InDarkYellow(), "     ", "Use default console for this help page".InRed())
                        //.Write("  --verbose  ".InDarkYellow(), "     ", "Include extra debug output".InCyan());
                    isOpts = false;
                }
                console.Write(FG.Green, value);//.InGreen());
                if (value == "Options:")
                    isOpts = true;
                if (value == "Usage:")
                    isUsage = true;
            }
        }
        else
        {
            if (isUsage && !string.IsNullOrWhiteSpace(value))
            {
                var bits = value.Trim().Split(" ");
                isUsage = false;
            }

            console.Write(value);
        }

        isNewline = value.EndsWith("\n");
    }

    private bool IsProcessingCommands(string value) =>
        value == "Commands:" || commandWriter.IsActive;
}

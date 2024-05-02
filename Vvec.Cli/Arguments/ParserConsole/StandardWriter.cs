using System.CommandLine.IO;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class StandardWriter : IStandardStreamWriter
{
    private readonly CommandWriter commandWriter;
    private readonly IConsole console = VConsole.Instance;

    private int callCount = 0;

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

    private bool isNewline = true;
    //private KeyValuePair<string, List<string>> currentGroup;
    private List<KeyValuePair<string, List<string>>> groups;
    private int groupIndex = 0;

    //public StandardWriter(Dictionary<string, List<string>> groups)
    //{
    //    this.groups = groups;
    //    currentGroup = groups.FirstOrDefault();
    //    if (currentGroup.Value is null)
    //        currentGroup = new KeyValuePair<string, List<string>>(null, new List<string>());
    //}

    public StandardWriter(List<KeyValuePair<string, List<string>>> groups)
    {
        this.groups = groups;
        commandWriter = new CommandWriter(groups);
    }

    private void WriteSegment(string value)
    {
        if (isNewline)
        {
            //if (DoExtraHaxx(value))
                if (value.StartsWith(" "))
                    console.Write(value.InDarkYellow());
                else
                {
                    console.Write(value.InGreen());
                    if (value.StartsWith("Options:"))
                    {
                        console
                            .WriteLine()
                            .WriteLine("  --dc       ".InDarkYellow(), "     ", "Use default console for this help page".InRed())
                            .Write("  --verbose  ".InDarkYellow(), "     ", "Include extra debug output".InCyan());
                    }
                }
        }
        else
            console.Write(value);

        isNewline = value.EndsWith("\n");
    }

    private bool IsProcessingCommands(string value) =>
        value == "Commands:" || commandWriter.IsActive;

    //private bool processingCommands = false;
    //private bool DoExtraHaxx(string value)
    //{

    //    if (groups.Any() && groupIndex < (groups.Count - 1))
    //    {
    //        if (value == "Commands:")
    //        {
    //            processingCommands = true;
    //            console.Write(groups[0].Key.InGreen());
    //            return false;
    //        }

    //        //if (!processingCommands)
    //        //    return true;

    //        var trimmedValue = value;
    //        if (value.Length > 2)
    //        {
    //            var indexOfSpace = value.IndexOf(" ", 2);
    //            trimmedValue = value.Substring(0, indexOfSpace == -1 ? value.Length : indexOfSpace);
    //        }

    //        if (groups[groupIndex + 1].Value.Contains(trimmedValue))
    //        {
    //            groupIndex++;
    //            console.WriteLine((groups[groupIndex].Key + ":").InGreen());
    //        }
    //    }

    //    if (value.StartsWith(" ") && value.Contains("<") && /* value.Contains("|") && */ value.Contains(">"))
    //    {
    //        PrintOptionWithArgs(value);
    //        return false;
    //    }

    //    return true;
    //}

    //private void PrintOptionWithArgs(string value)
    //{
    //    var bits = value.Trim().Split(" ");
    //    console.Write("  ");
    //    var count = 0;
    //    foreach (var bit in bits)
    //    {
    //        if (!bit.StartsWith("<"))
    //            console.Write(bit.InDarkYellow());
    //        else
    //            PrintArg(bit);

    //        if (count++ < (bits.Length - 1))
    //            console.Write(" ");
    //    }
    //}

    //private void PrintArg(string value)
    //{
    //    var options = value.Substring(1, value.Length - 2).Split("|");

    //    console.Write("<".InDarkYellow());
    //    for (int i = 0; i < options.Length - 1; i++)
    //        console.Write(options[i].InYellow(), "|".InDarkYellow());

    //    console.Write(options[options.Length - 1].InYellow());
    //    console.Write(">".InDarkYellow());
    //}
}

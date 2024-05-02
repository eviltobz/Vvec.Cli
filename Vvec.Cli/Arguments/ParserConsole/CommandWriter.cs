using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class CommandWriter
{
    private readonly List<KeyValuePair<string, List<string>>> groups;
    private readonly IConsole console = VConsole.Instance;
    private const string Indent = "  ";

    private bool isNewline = true;

    public bool IsActive { get; private set; }

    public CommandWriter(List<KeyValuePair<string, List<string>>> groups)
    {
        this.groups = groups;
    }

    //private List<IEnumerable<Coloured>> commandLines = new List<IEnumerable<Coloured>>();

    private List<ICommandOutput> commandLines = new List<ICommandOutput>();
    //private void AddNewCommandLine(Coloured item) => console.WriteLine(item); // commandLines.Add(new[] { item });
    private void DoWriteGroupTitle(string title) => commandLines.Add(new CommandHeading(title));

    private bool previousWasNewline = false;
    public void Write(string value)
    {
        if (!IsActive)
        {
            WriteGroupTitle(value);
            IsActive = true;
            return;
        }

        if (value == "\r")
            return;

        if (value == "\n" || value == "\r\n")
        {
            //int descriptionIndent = 20;
            //var bits = currentCommandOutput.GetFormattedSegments(descriptionIndent, Console.BufferWidth);


            //console.Write("/".InMagenta());
            //console.WriteLine();
            if(currentCommandOutput is not null)
                commandLines.Add(currentCommandOutput);
            currentCommandOutput = null;
        }
        else if (isNewline)
        {
            WriteGroupTitle(value);
            WriteCommand(value);
            previousWasNewline = false;
        }
        else
        {
            WriteDescription(value!);
            previousWasNewline = false;
        }
        isNewline = value.EndsWith("\n");

        if (isNewline && previousWasNewline)
            WriteOutput();

        previousWasNewline = isNewline;
    }

    private void WriteOutput()
    {
        if (!commandLines.Any())
            return;

        var longestCommand = commandLines.Max(l => l.CommandLength);
        var longestDescription = commandLines.Max(l => l.DescriptionLength);
        var fullWidth = Console.BufferWidth;

        var descriptionIndent = 55;
        if (longestCommand + longestDescription + 2 <= fullWidth)
            descriptionIndent = longestCommand + 2;
        else if ((longestCommand * 2) + 2 <= fullWidth)
            descriptionIndent = longestCommand + 2;
        else
            descriptionIndent = fullWidth / 2;

        foreach(var command in commandLines)
        {
            foreach (var bit in command.GetFormattedSegments(descriptionIndent, Console.BufferWidth))
                console.Write(bit);
	    }

        commandLines.RemoveRange(0, commandLines.Count);
    }

    //private void WriteCommand(string value)
    //{

    //    PrintOptionWithArgs(value);
    //    return;
    //    var (command, args) = SplitCommand(value);

    //    console.Write("-".InBlue());
    //    console.Write(Indent, command.InDarkYellow());

    //    for (int i = 0; i < args.Length; i++)
    //    {
    //        console.Write(" ", args[i].InCyan());
    //    }

    //    if (value.StartsWith(" ") && value.Contains("<") && /* value.Contains("|") && */ value.Contains(">"))
    //    {
    //        PrintOptionWithArgs(value);
    //    }
    //}

    private (string command, string[] args) SplitCommand(string value)
    {
        var split = value.Trim().Split(" ");
        return (split.First(), split.Skip(1).ToArray());
    }

    private CommandOutput currentCommandOutput;
    private void WriteDescription(string value)
    {
        //console.Write("!".InRed());
        //console.Write(value);
        currentCommandOutput.AddSegment(value);
        //console.Write("*".InGrey());
    }

    private int groupIndex = 0;

    private void WriteGroupTitle(string value)
    {
        if (!IsActive && !groups.Any()) // Default heading
            //console.Write(value.InGreen());
            DoWriteGroupTitle("Commands:");

        if (groups.Any() && (groupIndex < (groups.Count - 1) || !IsActive))
        {
            if (value == "Commands:")
            {
                //console.Write((groups[0].Key + ":").InGreen());
                DoWriteGroupTitle((groups[0].Key + ":"));
                return;
            }

            //if (!processingCommands)
            //    return true;

            var trimmedValue = value;
            if (value.Length > 2)
            {
                var indexOfSpace = value.IndexOf(" ", 2);
                trimmedValue = value.Substring(0, indexOfSpace == -1 ? value.Length : indexOfSpace);
            }

            if (groups[groupIndex + 1].Value.Contains(trimmedValue))
            {
                groupIndex++;
                //console.WriteLine((groups[groupIndex].Key + ":").InGreen());
                DoWriteGroupTitle((groups[groupIndex].Key + ":"));
            }
        }
    }


    private void WriteCommand(string value)
    {
        currentCommandOutput = new CommandOutput();
        currentCommandOutput.AddSegment(value);
        //var bits = value.Trim().Split(" ");
        //console.Write("  ");
        //var count = 0;
        //foreach (var bit in bits)
        //{
        //    if (!bit.StartsWith("<"))
        //        console.Write(bit.InDarkYellow());
        //    else
        //        PrintArg(bit);

        //    if (count++ < (bits.Length - 1))
        //        console.Write(" ");
        //}
    }

    //private void PrintArg(string value)
    //{
    //    var options = value.Substring(1, value.Length - 2).Split("|");

    //    console.Write("<".InCyan());
    //    for (int i = 0; i < options.Length - 1; i++)
    //        console.Write(options[i].InYellow(), "|".InCyan());

    //    console.Write(options[options.Length - 1].InYellow());
    //    console.Write(">".InCyan());
    //}
}

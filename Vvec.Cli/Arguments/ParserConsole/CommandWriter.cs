using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class CommandWriter
{
    private readonly List<KeyValuePair<string, List<string>>> groups;
    private readonly IConsole console = VConsole.Instance;
    private const string Indent = "  ";

    private bool isNewline = true;
    private CommandOutput currentCommandOutput;
    private int groupIndex = 0;

    public bool IsActive { get; private set; }

    public CommandWriter(List<KeyValuePair<string, List<string>>> groups)
    {
        this.groups = groups;
    }

    private List<ICommandOutput> commandLines = new List<ICommandOutput>();

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

    private (string command, string[] args) SplitCommand(string value)
    {
        var split = value.Trim().Split(" ");
        return (split.First(), split.Skip(1).ToArray());
    }


    private void WriteDescription(string value)
    {
        currentCommandOutput.AddSegment(value);
    }

    private void WriteGroupTitle(string value)
    {
        if (!IsActive && !groups.Any()) // Default heading
            DoWriteGroupTitle("Commands:");

        if (groups.Any() && (groupIndex < (groups.Count - 1) || !IsActive))
        {
            if (value == "Commands:")
            {
                DoWriteGroupTitle((groups[0].Key + ":"));
                return;
            }

            var trimmedValue = value;
            if (value.Length > 2)
            {
                var indexOfSpace = value.IndexOf(" ", 2);
                trimmedValue = value.Substring(0, indexOfSpace == -1 ? value.Length : indexOfSpace);
            }

            if (groups[groupIndex + 1].Value.Contains(trimmedValue))
            {
                groupIndex++;
                DoWriteGroupTitle((groups[groupIndex].Key + ":"));
            }
        }
    }

    private void WriteCommand(string value)
    {
        currentCommandOutput = new CommandOutput();
        currentCommandOutput.AddSegment(value);
    }
}

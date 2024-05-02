using System.Collections;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments.ParserConsole;

public class CommandElement : IEnumerable<Coloured>
{
    private readonly List<Coloured> parts = new List<Coloured>();

    public CommandElement() { }

    public CommandElement(Coloured part) 
    {
        Add(part);
    }

    public CommandElement Add(Coloured part)
    {
        parts.Add(part);
        return this;
    }

    public CommandElement Add(CommandElement element)
    {
        foreach (var part in element)
            Add(part);
        return this;
    }

    public IEnumerator<Coloured> GetEnumerator() => parts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => parts.GetEnumerator();

    public int Length =>
        parts.Sum(x => x.Value.Length);
}

public class CommandHeading : ICommandOutput
{
    private readonly string heading;

    public CommandHeading(string heading)
    {
        this.heading = heading + Environment.NewLine;
    }

    public int CommandLength => 0;

    public int DescriptionLength => 0;

    public IEnumerable<Coloured> GetFormattedSegments(int descriptionIndent, int maxWidth)
    {
        yield return heading.InGreen();
    }
}

public interface ICommandOutput
{
    IEnumerable<Coloured> GetFormattedSegments(int descriptionIndent, int maxWidth);

    int CommandLength { get; }

    int DescriptionLength { get; }
}

public class CommandOutput : ICommandOutput
{
    //private string? command;
    //private CommandElement[] args = new CommandElement[0];
    private CommandElement[] command;// = new CommandElement[0];
    //private Coloured[][] args = new Coloured[0][];
    private string? description;

    public void AddSegment(string? segment)
    {
        if (command is null)
        {
            var tokens = segment.Split(" ").Where(s => s != "");
            //command = tokens.First();
            //args = tokens.Skip(1).Select(FormatArgument).ToArray();
            command = tokens.Select(FormatCommandToken).ToArray();
        }
        else if (description is null)
            description = segment?.Trim();
    }

    private static readonly Coloured CommandIndent = "  ".InGrey();

    private CommandElement FormatCommandToken(string value)
    {
        if (!value.StartsWith("<"))
            return new CommandElement()
                //.Add(CommandIndent)
		        .Add(value.InDarkYellow());

        return FormatArgument(value);
    }

    //private Coloured[] FormatArgument(string value)
    private CommandElement FormatArgument(string value)
    {
        var options = value.Substring(1, value.Length - 2).Split("|");
        var retval = new CommandElement()
            .Add(" <".InCyan());

        for (int i = 0; i < options.Length - 1; i++)
            retval.Add(options[i].InYellow())
                .Add("|".InCyan());

        retval.Add(options[options.Length - 1].InYellow())
            .Add(">".InCyan());

        return retval; 
    }

    public IEnumerable<Coloured> GetFormattedSegments(int descriptionIndent, int maxWidth)
    {
        var commandLines = SplitCommandIntoLines(descriptionIndent - 2);
        var descriptionLines = SplitDescriptionIntoLines(maxWidth - descriptionIndent);

        var requiredDescLines = commandLines.Count - descriptionLines.Count;
        if (requiredDescLines > 0)
            for (int l = 0; l < requiredDescLines; l++)
                descriptionLines.Add("\n".InGrey());

        var requiredCommandLines = descriptionLines.Count - commandLines.Count;
        if (requiredCommandLines > 0)
            for (int l = 0; l < requiredCommandLines; l++)
                commandLines.Add(new CommandElement().Add(CommandIndent));



        for (int i = 0; i < commandLines.Count; i++)
        {
            foreach(var bit in commandLines[i])
            {
                yield return bit; 
	        }

            var requiredIndent = Math.Max(2, descriptionIndent - commandLines[i].Length);
            yield return new string(' ', requiredIndent).InGrey();

            yield return descriptionLines[i];
        }
    }

    private List<Coloured> SplitDescriptionIntoLines(int maxWidth)
    {
        maxWidth = Math.Max(maxWidth, 5); // stupidly small minimum default
        var remaining = description;
        var retval = new List<Coloured>();
        while (remaining is not null && remaining.Length > 0)
        {
            var maxToTake = Math.Min(remaining.Length, maxWidth);
            var take = maxToTake;
            if (maxToTake < remaining.Length)
            {
                while (remaining[(--take)] != ' ' && take > 0)
                { }
                if (take == 0)
                    //take = maxToTake;
                    take = remaining.Length; // window is too narrow, things are going to look borked
            }

            retval.Add((remaining.Substring(0, take) + "\n").InGrey());
            remaining = take >= remaining.Length
                ? ""
                : remaining.Substring(take);
        }
        return retval;

    }

    // Initially this is very similar to the Description version, but I expect this to become more complex 
    private List<CommandElement> SplitCommandIntoLines(int maxWidth)
    {
        //var remaining = command;
        //var retval = new List<Coloured>();
        //while (remaining.Length > 0)
        //{
        //    var take = Math.Min(remaining.Length, maxWidth);
        //    retval.Add(remaining.Substring(0, take).InDarkYellow());
        //    remaining = take >= remaining.Length
        //        ? ""
        //        : " " + remaining.Substring(take);
        //}
        //return retval;
        var retval = new List<CommandElement>();
        var currentLine = new CommandElement(CommandIndent);
        retval.Add(currentLine);
        var addedCommand = false; // if view is too narrow for the command, things will look borked
        foreach(var element in command)
        {
            if (currentLine.Length + element.Length <= maxWidth || !addedCommand)
            {
                currentLine.Add(element);
                addedCommand = true;
            }
            else
            {
                currentLine = new CommandElement(CommandIndent)
                    //.Add(CommandIndent)
                    .Add(element);
                retval.Add(currentLine);
            }
	    }
        return retval;
    }

    //public (int commandLength, int descriptionLength) GetLengths() =>
    //    (command.Aggregate(2, (acc, com) => acc + com.Length), description?.Length ?? 0);

    public int CommandLength => command.Aggregate(2, (acc, com) => acc + com.Length);

    public int DescriptionLength => description?.Length ?? 0;


    //private Coloured GetFormattedDescription(int descriptionIndent)
    //{
    //    var commandLength = command.Length;
    //    var requiredIndent = descriptionIndent - commandLength;

    //    return (new string(' ', requiredIndent) + description + "\n").InGrey();
    //}
}

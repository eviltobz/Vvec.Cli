using System.Text;

namespace Vvec.Cli.UI;

public class TestConsole : IInternalConsole
{
    private readonly Func<IInternalConsole, object[], VConsole.PromptImplementation> createPrompt;
    private readonly Action<string> assertFail;
    private readonly List<string> writtenLines = new();

    private StringBuilder currentLine = new StringBuilder();
    private IEnumerator<string>? linesToRead;

    internal TestConsole(
        Func<IInternalConsole, object[], VConsole.PromptImplementation> createPrompt,
        Action<string> assertFail)
    {
        this.assertFail = assertFail;
        this.createPrompt = createPrompt;
    }

    public TestConsole SetupLinesToRead(params string[] lines)
    {
        return SetupLinesToRead(lines as IEnumerable<string>);
    }

    public TestConsole SetupLinesToRead(IEnumerable<string> lines)
    {
        linesToRead = lines.GetEnumerator();
        return this;
    }

    private void PreAssertWork()
    {
        if (currentLine.Length > 0)
            (this as IConsole).WriteLine();
    }

    public void AssertLineContains(params string[] expected)
    {
        PreAssertWork();

        var normalised = expected.Select(x => x.ToUpper());

        var matches = writtenLines.Any(l => normalised.All(n => l.ToUpper().Contains(n)));

        const string spacer = "----------";

        if (!matches)
        {
            var outputBuilder = new StringBuilder();
            foreach (var line in writtenLines)
                outputBuilder.AppendLine(line);
            var fullOutput = outputBuilder.ToString();
            var reportStrings = expected.Select(x => $"'{x}'");
            var plural = expected.Length > 1;
            assertFail($"The string{(plural ? "s" : "")} {string.Join(", ", reportStrings)} " +
                $"{(plural ? "were" : "was")} not written in a single line to the console. The full output was:"
                + Environment.NewLine + "-----START-----"
                + Environment.NewLine + fullOutput
                + "------END------");
        }
    }

    string? IInternalConsole.ReadLine()
    {
        linesToRead.MoveNext();
        return linesToRead.Current;
    }

    IConsole.IPrompt IConsole.StartPrompt(params object[] items)
    {
        return new VConsole.PromptImplementation(this, items);
    }

    IConsole IConsole.Write(params object[]? items)
    {
        AppendToCurrentLine(items);
        return this;
    }
    IConsole IConsole.WriteLine(params object[]? items)
    {
        AppendToCurrentLine(items);
        (this as IConsole).WriteLine();
        return this;
    }

    private void AppendToCurrentLine(object[]? items)
    {
        foreach (var item in items)
        {
            //if (item is Coloured coloured)
            //    currentLine.Append(coloured.Value);
            //else
                currentLine.Append(item.ToString());
        }
    }

    IConsole IConsole.WriteLine()
    {
        writtenLines.Add(currentLine.ToString());
        currentLine = new StringBuilder();
        return this;
    }

    ConsoleKeyInfo IInternalConsole.ReadKey() => throw new NotImplementedException();
    //IInternalConsole IInternalConsole.DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true) => throw new NotImplementedException();
    IInternalConsole IInternalConsole.DoWrite(object[]? items, bool updateCurrentInfo = true) => throw new NotImplementedException();
    IConsole IConsole.Verbose => throw new NotImplementedException();
    void IConsole.AbortApplication(params object[] items) => throw new NotImplementedException();
    IConsole IConsole.ClearLine() => this;
    IConsole.IAppendableLine IConsole.StartAppendable(params object[] items) => throw new NotImplementedException();
}


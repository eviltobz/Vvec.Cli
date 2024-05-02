using System;
using Vvec.Cli.Arguments.ParserConsole;
using Vvec.Cli.UI;

namespace Vvec.Cli.Tests.Arguments.ParserConsole;

public class CommandOutputTests
{
    private CommandOutput sut;

    [SetUp]
    public void Setup()
    {
        sut = new CommandOutput();
    }

    [TestCase("  name", "  description", "\r", "\n")]
    [TestCase("  name", "  description", "\n")]
    public void SimpleCommandStructure(params string[] args)
    {
        foreach (var arg in args)
            sut.AddSegment(arg);

        var actual = CallGetFormattedSegments(8, 100);

        var expected = new Coloured[] {
            "  ".InGrey(),
            "name".InDarkYellow(),
            "  ".InGrey(),
            "description\n".InGrey(),
         };

        CollectionAssert.AreEqual(expected, actual);
    }

    [TestCase("  name", "  description", "\r", "\n")]
    [TestCase("  name", "  description", "\n")]
    public void SimpleCommandStructureWithDifferentIndent(params string[] args)
    {
        foreach (var arg in args)
            sut.AddSegment(arg);

        var actual = CallGetFormattedSegments(10, 100);

        var expected = new Coloured[] {
            "  ".InGrey(),
            "name".InDarkYellow(),
            "    ".InGrey(), // expect extra indent here
            "description\n".InGrey(), 
	     };

        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void CommandWithNullDescription()
    {
        sut.AddSegment("  name");

        var actual = CallGetFormattedSegments(10, 100);

        var expected = new Coloured[] {
            "  ".InGrey(),
            "name".InDarkYellow(),
            Padding(4),
            "\n".InGrey(), 
	     };

        AssertOutput(expected, actual);
    }

    [Test]
    public void CommandAndDescriptionTooBigToWrap()
    {
        sut.AddSegment("  1234567890123456789012345");
        sut.AddSegment("  ABCDEFGHIJABCDEFGHIJ");

        var actual = CallGetFormattedSegments(15, 26);

        var expected = new Coloured[] {
            "  ".InGrey(),
            "1234567890123456789012345".InDarkYellow(),
            "  ".InGrey(),
            "ABCDEFGHIJABCDEFGHIJ\n".InGrey(),
         };

        DebugPrint(expected, "expected");
        DebugPrint(actual, "Actual");

        CollectionAssert.AreEqual(expected, actual);
    }

    public static IEnumerable<object[]> LineWrapCases()
    {
        const string shortCommandWithEnumArgs = "  command <string> <some|enum|values>";
        const string shortCommandWithBasicArgs = "  command <string> <int>";
        int descriptionIndent = shortCommandWithEnumArgs.Length + 2;

        yield return new object[] {
            "no wrap",
            new[] {shortCommandWithEnumArgs, "some random blurb" },
            new Coloured[] {
                "  ".InGrey(),
                "command".InDarkYellow(),
                " <".InCyan(),
                "string".InYellow(),
                ">".InCyan(),
                " <".InCyan(),
                "some".InYellow(),
                "|".InCyan(),
                "enum".InYellow(),
                "|".InCyan(),
                "values".InYellow(),
                ">".InCyan(),
                "  ".InGrey(),
                "some random blurb\n".InGrey(),
             },
            descriptionIndent, 100
        };

        descriptionIndent = shortCommandWithEnumArgs.Length + 1;
        yield return new object[] {
            "arg wrap 1",
            new[] {shortCommandWithEnumArgs, "blurb" },
            new Coloured[] {
                CommandIndent,
                "command".InDarkYellow(),
                " <".InCyan(),
                "string".InYellow(),
                ">".InCyan(),
                Padding(descriptionIndent - 18),
                "blurb\n".InGrey(),

                CommandIndent,
                " <".InCyan(),
                "some".InYellow(),
                "|".InCyan(),
                "enum".InYellow(),
                "|".InCyan(),
                "values".InYellow(),
                ">".InCyan(),
                Padding(descriptionIndent - 21),
                "\n".InGrey(),
             },
            descriptionIndent, 100
        };

        descriptionIndent = "  command".Length + 3;
        yield return new object[] {
            "arg wrap 2",
            new[] {shortCommandWithBasicArgs, "blurb" },
            new Coloured[] {
                CommandIndent,
                "command".InDarkYellow(),
                Padding(3),
                "blurb\n".InGrey(),

                CommandIndent,
                " <".InCyan(),
                "string".InYellow(),
                ">".InCyan(),
                Padding(2),
                "\n".InGrey(),

                CommandIndent,
                " <".InCyan(),
                "int".InYellow(),
                ">".InCyan(),
                Padding(4),
                "\n".InGrey(),
             },
            descriptionIndent, 100
        };

        yield return new object[] {
            "desc wrap 1",
            new[] { "  meh", "some random blurb" },
            new Coloured[] {
                CommandIndent,
                "meh".InDarkYellow(),
                Padding(2),
                "some random\n".InGrey(),
                CommandIndent,
                Padding(5),
                " blurb\n".InGrey(),
             },
            7, 21
        };
    }

    private static Coloured Padding(int length) =>
        new string(' ', length).InGrey();

    private static Coloured CommandIndent => Padding(2);

    [TestCaseSource(nameof(LineWrapCases))]
    public void LineWrapTests(
        string title,
        string[] writes,
        Coloured[] expected,
        int descriptionIndent,
        int maxWidth)
    {
        foreach (var write in writes)
            sut.AddSegment(write);

        var actual = CallGetFormattedSegments(descriptionIndent, maxWidth);

        //DebugPrint(expected, "expected");
        //DebugPrint(actual, "Actual");

        //CollectionAssert.AreEqual(expected, actual);
        AssertOutput(expected, actual);
    }

    private Coloured[] CallGetFormattedSegments(int descriptionIndent, int maxWidth) =>
        sut.GetFormattedSegments(descriptionIndent, maxWidth).ToArray();

    [Test]
    public void CommandWithArguments()
    {
        const string command = "  command <string> <some|enum|values>";
        sut.AddSegment(command);
        sut.AddSegment("  blurb");

        var actual = CallGetFormattedSegments(command.Length + 2, 100);

        var expected = new Coloured[] {
            "  ".InGrey(),
            "command".InDarkYellow(),
            " <".InCyan(),
            "string".InYellow(),
            ">".InCyan(),
            " <".InCyan(),
            "some".InYellow(),
            "|".InCyan(),
            "enum".InYellow(),
            "|".InCyan(),
            "values".InYellow(),
            ">".InCyan(),

            "  ".InGrey(),
            "blurb\n".InGrey(),
         };

        //DebugPrint(expected, "expected");
        //DebugPrint(actual, "Actual");

        //CollectionAssert.AreEqual(expected, actual);
        AssertOutput(expected, actual);
    }

    [TestCase("command", "description", 9, 11)]
    [TestCase("  this", "  that", 6, 4)]
    [TestCase("this", "", 6, 0)]
    [TestCase("  something", null, 11, 0)]
    public void GetLengths_ReturnsLengthsOfSections(string command, string description, int expectedCommandLength, int expectedDescriptionLength)
    {
        sut.AddSegment(command);
        sut.AddSegment(description);

        var commandLength = sut.CommandLength;
        var descriptionLength = sut.DescriptionLength;

        Assert.AreEqual(expectedCommandLength, commandLength);
        Assert.AreEqual(expectedDescriptionLength, descriptionLength);
    }


    private void DebugPrint(IEnumerable<Coloured> output, string? title = null)
    {
        if (title is null)
            Console.WriteLine("--------DEBUG PRINT--------");
        else
            Console.WriteLine($"--------DEBUG PRINT {title.ToUpper()}--------");
        for (int i = 0; i < output.Count(); i++)
        {
            var item = output.ElementAt(i);
            Console.Write($"'{i}");
            if (item.Value.Trim().Length == 0 && item.Value.Length > 0 && item.Value != "\n")
                Console.Write($"[{item.Value.Length}]");
            else
                Console.Write(output.ElementAt(i).Value);
        }
        //foreach (var entry in output)
        //    Console.Write(entry.Value);
        Console.WriteLine("--------END PRINT--------");
    }

    private void AssertOutput(Coloured[] expected, Coloured[] actual)
    { 
        DebugPrint(expected, "expected");
        DebugPrint(actual, "Actual");

        CollectionAssert.AreEqual(expected, actual);
    }


}


using System.CommandLine.IO;
using Vvec.Cli.UI;

namespace Vvec.Cli.Arguments
{
    /// This is nasty & hacky, but _so far_ it seems to work ok :)
    /// Testing Notes:
    /// The following are different command lines to test with that should have differences in their output that could show up problems
    /// dz
    /// dz /?
    /// dz start /?
    /// dz --version

    public class ArgumentParserConsole : System.CommandLine.IConsole
    {
        private StandardWriter standard = new StandardWriter();
        private ErrorWriter error = new ErrorWriter();
        public IStandardStreamWriter Out => standard;

        public bool IsOutputRedirected => false;

        public IStandardStreamWriter Error => error;

        public bool IsErrorRedirected => false;

        public bool IsInputRedirected => false;
    }

    public class ErrorWriter : IStandardStreamWriter
    {
        private IConsole console = VConsole.Instance;
        public void Write(string? value)
        {
            console.Write(value.InDarkRed());
        }
    }

    public class StandardWriter : IStandardStreamWriter
    {
        private IConsole console = VConsole.Instance;
        private int callCount = 0;
        public void Write(string? value)
        {
            if (callCount == 0)
            {

                console.WriteLine("Use '-dc' switch as the first arg to force the default console instead of my colour & formatty console for help output".InDarkGreen());

                console.WriteLine("-- Using hacky output colouriser. This needs genericising --".InGrey().OnDarkGrey())
                    .WriteLine("Also note, the default console can deal with command descriptions wrapping & indents them! Things go horky borky if you then resize, but when you're just using it, it looks a lot betterer.".InDarkMagenta());
            }

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
                WriteSegment(value);
            }
        }

        private bool isNewline = true;
        private void WriteSegment(string value)
        {
            if (isNewline)
            {
                if (DoExtraHaxx(value))
                    if (value.StartsWith(" "))
                        console.Write(value.InDarkYellow());
                    else
                    {
                        console.Write(value.InGreen());
                        if (value.StartsWith("Options:"))
                        {
                            console
                                .WriteLine()
                                .WriteLine("  --dc       ".InDarkYellow(), "Use default console for this help page".InRed())
                                .Write("  --verbose  ".InDarkYellow(), "Include extra debug output".InCyan());
                        }
                    }
            }
            else
                console.Write(value);

            isNewline = value.EndsWith("\n");
        }

        // This could be expanded on to be more pluggable if I try to pull these bits out into something reusable.
        // * Dictionary of values, and the thing to do first if the value is found?
        // * Link up to the way that commands are created more, so I can group them & title them appropriately?
        // * * Hmmm, sounds like a cunning plan...
        private bool DoExtraHaxx(string value)
        {
            if (value == "  colours")
            {
                console.WriteLine("Hacking & Tweaking Commands:".InGreen());
                return true;
            }

            if (value.StartsWith(" ") && value.Contains("<") && value.Contains("|") && value.Contains(">"))
            {
                PrintOptionWithEnum(value);
                return false;
            }

            if (value == "Commands:")
            {
                console.Write("Main Commands:".InGreen());
                return false;
            }

            return true;
        }

        private void PrintOptionWithEnum(string value)
        {
            var first = value.Split("<");
            var second = first[1].Split(">");
            var options = second[0].Split("|");

            console.Write(first[0].InDarkYellow());
            console.Write("<".InDarkYellow());
            for (int i = 0; i < options.Length - 1; i++)
                console.Write(options[i].InYellow(), "|".InDarkYellow());

            console.Write(options[options.Length - 1].InYellow());
            console.Write(">".InDarkYellow());
            console.Write(second[1].InDarkYellow());
        }
    }
}

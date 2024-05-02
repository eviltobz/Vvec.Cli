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

    public class ErrorWriter : IStandardStreamWriter
    {
        private IConsole console = VConsole.Instance;
        private int writeCount = 0;
        private HashSet<string> errors = new HashSet<string>();

        public void Write(string? value)
        {
            if (value == "Required command was not provided.\r\n"
                || (value == "\r\n" && writeCount == 0)
                || errors.Contains(value))
                return;

            console.Write(value.InDarkRed());
            writeCount++;
            errors.Add(value);
        }
    }

    public class StandardWriter : IStandardStreamWriter
    {
        private readonly CommandWriter commandWriter;
        private readonly IConsole console = VConsole.Instance;

        private int callCount = 0;

        public void Write(string? value)
        {
            if (callCount == 0)
            {
                console
                    .WriteLine("Note, the default console can deal with command descriptions wrapping & indents them! Things go horky borky if you then resize, but when you're just using it, it looks a lot betterer.".InDarkMagenta());
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
                if (processingCommands)
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
                                .WriteLine("  --dc       ".InDarkYellow(), "     ", "Use default console for this help page".InRed())
                                .Write("  --verbose  ".InDarkYellow(), "     ", "Include extra debug output".InCyan());
                        }
                    }
            }
            else
                console.Write(value);

            isNewline = value.EndsWith("\n");
        }


        private bool processingCommands = false;
        private bool DoExtraHaxx(string value)
        {

            if (groups.Any() && groupIndex < (groups.Count - 1))
            {
                if (value == "Commands:")
                {
                    processingCommands = true;
                    console.Write(groups[0].Key.InGreen());
                    return false;
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
                    console.WriteLine((groups[groupIndex].Key + ":").InGreen());
                }
            }

            if (value.StartsWith(" ") && value.Contains("<") && /* value.Contains("|") && */ value.Contains(">"))
            {
                PrintOptionWithArgs(value);
                return false;
            }

            return true;
        }

        private void PrintOptionWithArgs(string value)
        {
            var bits = value.Trim().Split(" ");
            console.Write("  ");
            var count = 0;
            foreach (var bit in bits)
            {
                if (!bit.StartsWith("<"))
                    console.Write(bit.InDarkYellow());
                else
                    PrintArg(bit);

                if (count++ < (bits.Length - 1))
                    console.Write(" ");
            }
        }

        private void PrintArg(string value)
        {
            var options = value.Substring(1, value.Length - 2).Split("|");

            console.Write("<".InDarkYellow());
            for (int i = 0; i < options.Length - 1; i++)
                console.Write(options[i].InYellow(), "|".InDarkYellow());

            console.Write(options[options.Length - 1].InYellow());
            console.Write(">".InDarkYellow());
        }
    }

    public class CommandWriter
    {
        private readonly List<KeyValuePair<string, List<string>>> groups;
        private readonly IConsole console = VConsole.Instance;
        private const string Indent = "  ";

        private bool isNewline = true;

        public CommandWriter(List<KeyValuePair<string, List<string>>> groups)
        {
            this.groups = groups;
        }

        public void Write(string? value)
        {
            if (value == Environment.NewLine)
            {
                console.WriteLine();
            }
            else if (isNewline)
            {
                WriteCommand(value!);
            }
            else
            {
                WriteDescription(value!);
            }
            isNewline = value.EndsWith("\n");
        }

        private void WriteCommand(string value)
        {
            var (command, args) = SplitCommand(value);

            console.Write("-".InBlue());
            console.Write(Indent, command.InDarkYellow());

            for (int i = 0; i < args.Length; i++)
            {
                console.Write(" ", args[i].InCyan());
            }
        }

        private (string command, string[] args) SplitCommand(string value)
        {
            var split = value.Trim().Split(" ");
            return (split.First(), split.Skip(1).ToArray());
        }

        private void WriteDescription(string value)
        {
            console.Write("!".InRed());
            console.Write(value);
        }
    }
}

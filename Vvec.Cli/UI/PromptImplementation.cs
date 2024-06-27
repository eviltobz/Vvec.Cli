//using DaznCli.Enums;

namespace Vvec.Cli.UI;


public partial class VConsole
{
    internal class PromptImplementation : IConsole.IPrompt
    {
        private readonly IInternalConsole console;
        private readonly List<List<object>> lines = new();

        public PromptImplementation(IInternalConsole console, params object[] items)
        {
            this.console = console;
            lines.Add(items.ToList());
        }

        public IConsole.IPrompt AddLine(params object[] items)
        {
            lines.Add(items.ToList());
            return this;
        }

        private void AppendPrompt(params object[] items)
        {
            lines.Last().AddRange(items);
        }

        private void AppendPromptMatchingStyle(string message)
        {
            var finalBitOfPrompt = lines.Last().Last();

            if (finalBitOfPrompt is Coloured coloured)
                AppendPrompt(new Coloured(message, coloured.Foreground, coloured.Background));
            else
                AppendPrompt(message);
        }

        public void PressAnyKey(string? customPrompt = null)
        {
            AppendPrompt(customPrompt ?? "Press any key to continue.");
            WritePrompt();
            console.ReadKey();
        }

        public string GetFreeText(Predicate<string?>? validator = null)
        {
            string input = DoGetFreeText(validator);

            ShowFinalPromptAndInput(input, 1);

            return input;
        }

        private string DoGetFreeText(Predicate<string>? validator)
        {
            AppendPromptMatchingStyle(": ");
            WritePrompt();
            var input = console.ReadLine() ?? string.Empty;
            if (validator is not null)
            {
                while (!validator(input))
                {
                    RePromptForInvalidInput(input, 1);
                    input = console.ReadLine() ?? string.Empty;
                }
            }

            return input;
        }

        public (bool isDefault, string value) GetFreeTextOrDefault(string defaultValue, Predicate<string?>? validator = null)
        {
            var input = DoGetFreeText(validator);
            var isDefault = string.IsNullOrEmpty(input);
            if (isDefault)
            {
                input = defaultValue;
                ShowFinalPromptAndInput("<DEFAULT>", 1);
            }
            else
                ShowFinalPromptAndInput(input, 1);

            return (isDefault, input);
        }


        private IInternalConsole WritePrompt()
        {
            for (int i = 0; i < lines.Count - 1; i++)
                console.WriteLine(lines[i].ToArray());
            console.Write(lines.Last().ToArray());
            return console;
        }

        TEnum IConsole.IPrompt.GetEnumSelection<TEnum>(TEnum? defaultValue, Colour? optionsColour, Colour? defaultValueColour)
        {
            var highest = Enum.GetValues(typeof(TEnum)).Cast<int>().OrderByDescending(x => x).First();
            if (highest > 9)
            {
                console.WriteLine()
                    .WriteLine($"Enum \"{typeof(TEnum).Name}\" goes beyond single digits. Highest value is {highest}. Select method needs extending to work with this!".InDarkRed().OnYellow());
            }

            foreach (var option in Enum.GetValues(typeof(TEnum)))
            {
                var colour = defaultValue.HasValue && option.Equals(defaultValue) ? defaultValueColour : optionsColour;
                console.DoWrite($"{(option.Equals(defaultValue) ? ">>" : "  ")}", defaultValueColour)
                    .DoWrite($"({(int)option}) {option}", colour).WriteLine();
            }

            if (defaultValue.HasValue)
                AppendPromptMatchingStyle(" (Enter for default): ");
            else
                AppendPromptMatchingStyle(": ");

            WritePrompt();

            while (true)
            {
                var selection = console.ReadKey(); // Need to scan the values and swap to a read string if there's double digits.
                                                   // or, if the key is the first in a valid multi thing, wait for another, if not do the error.
                                                   // if you get another and together they're valid - accept it, if not, do the error
                                                   // and yeah, we don't want triple digits for a selection here!

                TEnum? choice = null;
                Coloured? displayChoice = null;
                if (selection.Key == ConsoleKey.Enter && defaultValue.HasValue)
                {
                    choice = defaultValue.Value;
                    displayChoice = new(choice, defaultValueColour ?? optionsColour);
                }

                if (int.TryParse(selection.KeyChar.ToString(), out int value))
                {
                    var input = ParseEnumFromInt<TEnum>(value);
                    if (input.success)
                        choice = input.parsed;

                    displayChoice = new(choice, optionsColour);
                }

                if (choice.HasValue)
                {
                    ShowFinalPromptAndInput(displayChoice!);

                    return choice.Value;
                }
                RePromptForInvalidInput(selection.Key);
            }
        }

        private (bool success, TEnum parsed) ParseEnumFromInt<TEnum>(int value) where TEnum : struct, Enum
        {
            if (Enum.TryParse(value.ToString(), out TEnum parsed) && Enum.IsDefined(parsed))
                return (true, parsed);

            return (false, default(TEnum));
        }

        private void RePromptForInvalidInput(object input, int clearLinesOffset = 0)
        {
            //console.ClearLines(lines.Count + clearLinesOffset);
            //WritePrompt();

            console.ClearLine()
                .Write(lines.Last().ToArray())
                .Write("(\"".InRed()).Write(input.ToString()).Write("\" is invalid) :".InRed());
        }

        private void ShowFinalPromptAndInput(object input, int clearLinesOffset = 0)
        {
            //console.ClearLines(lines.Count + clearLinesOffset);
            //WritePrompt().WriteLine(input);

            console
                .ClearLine()
                .Write(lines.Last().ToArray())
                .WriteLine(input);
        }

        public YesNo GetConfirmation(Colour? optionsColour, bool caseSensitive = false)
        {
            AppendPrompt(new Coloured($" - [Y]es/[N]o{(caseSensitive ? " (Case-Sensitive)" : "")}: ", optionsColour));
            WritePrompt();

            while (true)
            {
                var keyInfo = console.ReadKey();
                var key = keyInfo.Key;

                YesNo? result = key switch
                {
                    ConsoleKey.Y => YesNo.Yes,
                    ConsoleKey.N => YesNo.No,
                    _ => null
                };

                var passedCaseCheck = !caseSensitive || (result.HasValue && (keyInfo.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift);

                if (result.HasValue && passedCaseCheck)
                {
                    ShowFinalPromptAndInput(result);
                    return result.Value;
                }

                RePromptForInvalidInput(key);
            }
        }
    }
}

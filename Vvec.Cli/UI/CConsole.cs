//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
////using DaznCli.Enums;

///// <summary>
///// Console Color Helper class that provides coloring to individual commands
///// </summary>
//public class CConsole : CConsole.ICConsole
//{
//    // Hmmm, should I have the static methods here? It's nice & convenient for any code to just wap something out onto the console
//    // but it is obviously test-proof. That's not _great_ for outputting stuff, but is particularly bad for input, and we've got
//    // a few input methods now as well.
//    public interface ICConsole
//    {
//        ICConsole WriteLine(string? text = "", ConsoleColor? colour = null);
//        ICConsole WriteLine(object item, ConsoleColor? colour = null);
//        ICConsole WriteLine();
//        ICConsole Write(string? text, ConsoleColor? color = null);
//        ICConsole Write(object item, ConsoleColor? color = null);
//        ICConsole ClearLine();
//        ICConsole ClearLines(int numberToClear);
//        void ClearWaitingKeys();
//        ConsoleKeyInfo ReadKey();
//        string? ReadLine();

//        TEnum Select<TEnum>(
//            string prompt,
//            TEnum? defaultValue,
//            ConsoleColor? promptColour,
//            ConsoleColor? optionsColour,
//            ConsoleColor? defaultColour)
//            where TEnum : struct, Enum;
//    }

//    private CConsole() { }

//    private static readonly ICConsole _instance = new CConsole();
//    public static ICConsole Instance { get => _instance; }

//    public static ICConsole WriteLine(string? text, ConsoleColor? colour = null) =>
//        _instance.WriteLine(text, colour);
//    public static ICConsole WriteLine() =>
//        _instance.WriteLine();
//    public static ICConsole Write(string? text, ConsoleColor? colour = null) =>
//       _instance.Write(text, colour);

//    public static ICConsole ClearLine() =>
//       _instance.ClearLine();
//    public static ICConsole ClearLines(int numberToClear) =>
//       _instance.ClearLines(numberToClear);

//    private readonly Stack<int> _previousLineLengths = new Stack<int>();
//    //private readonly List<int> _previousLineLengths = new List<int>();
//    private int _currentLineLength;

//    ICConsole ICConsole.WriteLine()
//        => WriteLine("");

//    ICConsole ICConsole.WriteLine(string? text, ConsoleColor? colour = null)
//    {
//        //// HAXX
//        //var start = Console.GetCursorPosition();
//        //text += $" ({start.Top}/{_previousLineLengths.Count})";

//        if (colour.HasValue)
//        {
//            var oldColor = System.Console.ForegroundColor;
//            if (colour == oldColor)
//                Console.WriteLine(text);
//            else
//            {
//                Console.ForegroundColor = colour.Value;
//                Console.WriteLine(text);
//                Console.ForegroundColor = oldColor;
//            }
//        }
//        else
//            Console.WriteLine(text);

//        _previousLineLengths.Push(_currentLineLength + (text is null ? 0 : text.Length));
//        _currentLineLength = 0;

//        return this;
//    }

//    ICConsole ICConsole.Write(string? text, ConsoleColor? colour = null)
//    {
//        if (colour.HasValue)
//        {
//            var oldColor = System.Console.ForegroundColor;
//            if (colour == oldColor)
//                Console.Write(text);
//            else
//            {
//                Console.ForegroundColor = colour.Value;
//                Console.Write(text);
//                Console.ForegroundColor = oldColor;
//            }
//        }
//        else
//            Console.Write(text);

//        _currentLineLength += text is null ? 0 : text.Length;
//        return this;
//    }

//    public void HAXX()
//    {
//        DimensionDump();
//        PrintChars(100);
//        DimensionDump();
//        PrintChars(200);
//        DimensionDump();
//        var x = 4;
//        CConsole.Write($"About to clear {x} lines").ReadKey();
//        ClearLines(x);

//    }


//    public static TEnum Select<TEnum>(
//        string prompt,
//        TEnum? defaultValue = null,
//        ConsoleColor? promptColour = null,
//        ConsoleColor? optionsColour = null,
//        ConsoleColor? defaultColour = null)
//        where TEnum : struct, Enum
//        => _instance.Select<TEnum>(prompt, defaultValue, promptColour, optionsColour, defaultColour);

//    TEnum ICConsole.Select<TEnum>(
//        string prompt,
//        TEnum? defaultValue,
//        ConsoleColor? promptColour,
//        ConsoleColor? optionsColour,
//        ConsoleColor? defaultColour)
//    {
//        CConsole.ClearWaitingKeys();

//        foreach(var option in Enum.GetValues(typeof(TEnum)))
//        {
//            CConsole.Write($"{(option.Equals(defaultValue) ? ">>" : "  ")}", defaultColour);
//            CConsole.WriteLine($"({(int)option}) {option}", optionsColour);
//        }

//        CConsole.Write(prompt, promptColour);

//        while (true)
//        {
//            var selection = CConsole.ReadKey(); // Need to scan the values and swap to a read string if there's double digits.
//            if(selection.Key == ConsoleKey.Enter && defaultValue.HasValue)
//                return defaultValue.Value;

//            if (int.TryParse(selection.KeyChar.ToString(), out int value))
//            {
//                var choice = ParseEnumFromInt<TEnum>(value);

//                //if (Enum.TryParse(typeof(TEnum), selection.KeyChar.ToString(), out var choice) && Enum.IsDefined(typeof(TEnum), choice))
//                if (choice.success)
//                {
//                    CConsole.ClearLines(Enum.GetValues(typeof(TEnum)).Length + 1);
//                    return choice.parsed;
//                }
//            }

//            CConsole.ClearLine()
//                .Write("Option \"", ConsoleColor.Red).Write(selection.KeyChar.ToString()).Write("\" is invalid. ", ConsoleColor.Red)
//                .Write(prompt, promptColour);
//        }
//    }

//        private (bool success, TEnum parsed) ParseEnumFromInt<TEnum>(int value) where TEnum : struct, Enum
//        {
//            if (Enum.TryParse(value.ToString(), out TEnum parsed) && Enum.IsDefined(parsed))
//                return (true, parsed);

//            return (false, default(TEnum));
//        }


//    private void PrintChars(int number)
//    {
//        var i = 0;
//        for (int character = 0; character < 10; character++)
//        {
//            CConsole.Write(character.ToString());
//            if (character == 9)
//                character = 0;
//            i++;
//            if (i == number)
//                break;
//        }
//        CConsole.WriteLine();
//    }

//    private void DimensionDump()
//    {
//        CConsole.WriteLine($"Buffer:{Console.BufferWidth} x {Console.BufferHeight}. Window:{Console.WindowWidth} x {Console.WindowHeight}. Largest Window:{Console.LargestWindowWidth} x {Console.LargestWindowHeight}. Current:{Console.CursorLeft} x {Console.CursorTop}.");
//        CConsole.WriteLine("Lines: " + string.Join(", ", _previousLineLengths.Reverse()));
//    }

//    public ICConsole WriteLine(object item, ConsoleColor? colour = null)
//        => WriteLine(item?.ToString(), colour);

//    public ICConsole Write(object item, ConsoleColor? color = null)
//        => Write(item.ToString(), color);

//    ICConsole ICConsole.ClearLine()
//    {
//        var wrappedLines = new List<int>();
//        var chunk = _currentLineLength;
//        while (chunk >= Console.BufferWidth)
//        {
//            wrappedLines.Add(Console.BufferWidth - 1);
//            chunk -= Console.BufferWidth;
//        }
//        //wrappedLines.Add(chunk);
//        var clearText = "".PadLeft(Console.BufferWidth , ' ');

//        foreach (var line in wrappedLines)
//        {
//            var startW = Console.GetCursorPosition();
//            Console.SetCursorPosition(0, startW.Top);
//            Console.Write(clearText);
//            Console.SetCursorPosition(0, startW.Top - 1);
//        }

//        var start = Console.GetCursorPosition();
//        Console.SetCursorPosition(0, start.Top);
//        Console.Write(clearText);
//        Console.SetCursorPosition(0, start.Top);
//        _currentLineLength = 0;
//        return this;
//    }

//    ICConsole ICConsole.ClearLines(int numberToClear)
//    {
//        if (numberToClear > _previousLineLengths.Count + 1)
//            throw new ArgumentException($"Asked to clear {numberToClear} lines, but there are only {_previousLineLengths.Count + 1}");

//        if (numberToClear < 1)
//            throw new ArgumentException($"Asked to clear {numberToClear} lines. Need a number >= 1");

//        ClearLine();
//        for (int i = 1; i < numberToClear; i++)
//        {
//            _currentLineLength = _previousLineLengths.Pop();
//            var current = Console.GetCursorPosition();
//            Console.SetCursorPosition(0, current.Top - 1);
//            ClearLine();
//        }

//        return this;
//    }


//    public static void ClearWaitingKeys() => _instance.ClearWaitingKeys();
//    void ICConsole.ClearWaitingKeys()
//    {
//        while (Console.KeyAvailable)
//        {
//            Console.ReadKey();
//        }
//    }

//    public static ConsoleKeyInfo ReadKey() => _instance.ReadKey();
//    ConsoleKeyInfo ICConsole.ReadKey()
//    {
//        var start = Console.GetCursorPosition();
//        var retval = Console.ReadKey();
//        var end = Console.GetCursorPosition();
//        if (end.Left > start.Left)
//            _currentLineLength++;
//        else
//        {
//            _previousLineLengths.Push(_currentLineLength);
//            _currentLineLength = 0;
//        }
//        return retval;
//    }

//    public static string? ReadLine() => _instance.ReadLine();
//    string? ICConsole.ReadLine()
//    {
//        var retval = Console.ReadLine();

//        _previousLineLengths.Push(_currentLineLength + retval!.Length);
//        _currentLineLength = 0;

//        return retval;
//    }


//    //#region Wrappers and Templates


//    ///// <summary>
//    ///// Writes a line of header text wrapped in a in a pair of lines of dashes:
//    ///// -----------
//    ///// Header Text
//    ///// -----------
//    ///// and allows you to specify a color for the header. The dashes are colored
//    ///// </summary>
//    ///// <param name="headerText">Header text to display</param>
//    ///// <param name="wrapperChar">wrapper character (-)</param>
//    ///// <param name="headerColor">Color for header text (yellow)</param>
//    ///// <param name="dashColor">Color for dashes (gray)</param>
//    //public ColourConsole WriteWrappedHeader(string headerText,
//    //                                        char wrapperChar = '-',
//    //                                        ConsoleColor headerColor = ConsoleColor.Yellow,
//    //                                        ConsoleColor dashColor = ConsoleColor.DarkGray)
//    //{
//    //    if (string.IsNullOrEmpty(headerText))
//    //        return;

//    //    string line = new string(wrapperChar, headerText.Length);

//    //    WriteLine(line, dashColor);
//    //    WriteLine(headerText, headerColor);
//    //    WriteLine(line, dashColor);
//    //}

//    //private static Lazy<Regex> colorBlockRegEx = new Lazy<Regex>(
//    //    () => new Regex("\\[(?<color>.*?)\\](?<text>[^[]*)\\[/\\k<color>\\]", RegexOptions.IgnoreCase),
//    //    isThreadSafe: true);

//    ///// <summary>
//    ///// Allows a string to be written with embedded color values using:
//    ///// This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
//    ///// </summary>
//    ///// <param name="text">Text to display</param>
//    ///// <param name="baseTextColor">Base text color</param>
//    //public ColourConsole WriteEmbeddedColorLine(string text, ConsoleColor? baseTextColor = null)
//    //{
//    //    if (baseTextColor == null)
//    //        baseTextColor = Console.ForegroundColor;

//    //    if (string.IsNullOrEmpty(text))
//    //    {
//    //        WriteLine(string.Empty);
//    //        return;
//    //    }

//    //    int at = text.IndexOf("[");
//    //    int at2 = text.IndexOf("]");
//    //    if (at == -1 || at2 <= at)
//    //    {
//    //        WriteLine(text, baseTextColor);
//    //        return;
//    //    }

//    //    while (true)
//    //    {
//    //        var match = colorBlockRegEx.Value.Match(text);
//    //        if (match.Length < 1)
//    //        {
//    //            Write(text, baseTextColor);
//    //            break;
//    //        }

//    //        // write up to expression
//    //        Write(text.Substring(0, match.Index), baseTextColor);

//    //        // strip out the expression
//    //        string highlightText = match.Groups["text"].Value;
//    //        string colorVal = match.Groups["color"].Value;

//    //        Write(highlightText, colorVal);

//    //        // remainder of string
//    //        text = text.Substring(match.Index + match.Value.Length);
//    //    }

//    //    Console.WriteLine();
//    //}

//    //#endregion

//    //#region Success, Error, Info, Warning Wrappers

//    ///// <summary>
//    ///// Write a Success Line - green
//    ///// </summary>
//    ///// <param name="text">Text to write out</param>
//    //public ColourConsole WriteSuccess(string text)
//    //{
//    //    WriteLine(text, ConsoleColor.Green);
//    //}

//    ///// <summary>
//    ///// Write a Error Line - Red
//    ///// </summary>
//    ///// <param name="text">Text to write out</param>
//    //public ColourConsole WriteError(string text)
//    //{
//    //    WriteLine(text, ConsoleColor.Red);
//    //}

//    ///// <summary>
//    ///// Write a Warning Line - Yellow
//    ///// </summary>
//    ///// <param name="text">Text to Write out</param>
//    //public ColourConsole WriteWarning(string text)
//    //{
//    //    WriteLine(text, ConsoleColor.DarkYellow);
//    //}


//    ///// <summary>
//    ///// Write a Info Line - dark cyan
//    ///// </summary>
//    ///// <param name="text">Text to write out</param>
//    //public ColourConsole WriteInfo(string text)
//    //{
//    //    WriteLine(text, ConsoleColor.DarkCyan);
//    //}

//    //#endregion
//}

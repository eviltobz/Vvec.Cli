//using DaznCli.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Vvec.Cli.UI;
// Can we get some easy way to create levels for console writes (at least 2 for normal & verbose) then have some standard verbose flag in the CLI for extra info?
// Maybe have a top level IConsole & adds a Versbose property with an IConsoleWriter. Move all of the non-interactive writes to IConsoleWriter, & have IConsole implement it.
// VConsole can just return itself in the Verbose property if enabled, or a null object if not.
// Or would it just be better to duplicate up the Write/WriteLine & mebe StartAppendable with different level suffixes?
// Not sure I like the idea of including a WriteLevel enum in the Write calls... We're using (params object[]? items) so we couldn't add it as an optional that defaults to normal
// verbosity, so all calls would need to be modified to include it
public interface IConsole
{
    [Obsolete("Not obsolete, I just need to mess around with splitting the interfaces & stuff. See comment about IConsoleWriter")]
    IConsole Verbose { get; }
    IConsole Write(params object[]? items);
    IConsole WriteLine(params object[]? items);
    IConsole WriteLine();

    IConsole ClearLine();
    //IConsole ClearLines(int numberToClear);

    IPrompt StartPrompt(params object[] items);
    IAppendableLine StartAppendable(params object[] items);

    void Abort(params object[] items);

    public interface IPrompt
    {
        IPrompt AddLine(params object[] items);

        string GetFreeText(Predicate<string?>? validator = null);
        (bool isDefault, string value) GetFreeTextOrDefault(string defaultValue, Predicate<string?>? validator = null);

        TEnum GetEnumSelection<TEnum>(
            TEnum? defaultValue = null,
            Colour? optionsColour = null,
            Colour? defaultValueColour = null)
            where TEnum : struct, Enum;

        YesNo GetConfirmation(Colour? optionsColour = null, bool caseSensitive = false);
    }

    public interface IAppendableLine
    {
        IAppendableLine Write(params object[] items);
        IAppendableLine StartSpinner();
    }
}

public partial class VConsole : IConsole
{
    #region Singleton instantiation
    // VConsole needs to be a singleton so it can track what it has done if callers need to change previous output
    private VConsole() { }

    private static readonly IConsole _instance = new VConsole();

    public static IConsole Instance => _instance;

    //public IConsole Verbose { get; private set; } = _instance;

    //public static void SetVerbose()
    //{
    //    ((VConsole)_instance).Verbose = new NullWriter();
    //}
    public IConsole Verbose { get; private set; } = new NullWriter();

    public static void SetVerbose()
    {
        ((VConsole)_instance).Verbose = _instance;
    }

    #endregion

    private readonly Stack<int> _previousLineLengths = new Stack<int>();
    private int _currentLineLength;
    private static object locker = new object();

    private VConsole DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true)
    {
        lock (locker)
        {
            ConsoleColor originalForeground = default;
            ConsoleColor originalBackground = default;

            if (foregroundColour.HasValue)
            {
                originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = (ConsoleColor)foregroundColour.Value;
            }
            if (backgroundColour.HasValue)
            {
                originalBackground = Console.BackgroundColor;
                Console.BackgroundColor = (ConsoleColor)backgroundColour.Value;
            }

            Console.Write(text);

            if (foregroundColour.HasValue)
                Console.ForegroundColor = originalForeground;
            if (backgroundColour.HasValue)
                Console.BackgroundColor = originalBackground;

            if (updateCurrentInfo)
                _currentLineLength += text is null ? 0 : text.Length;
            return this;
        }
    }

    public IConsole Write(params object[]? items)
    {
        if (items is not null)
            foreach (var item in items)
            {
                if (item is Coloured coloured)
                    DoWrite(coloured.Value, coloured.Foreground, coloured.Background);
                else
                    DoWrite(item?.ToString());
            }
        return this;
    }
    public IConsole WriteLine(params object[]? items)
    {
        return Write(items).WriteLine();
    }
    public IConsole WriteLine()
    {
        lock (locker)
        {
            //foreach (var line in _appendableLines)
            //    line.Offset++;

            //_previousLineLengths.Push(_currentLineLength);
            //_currentLineLength = 0;
            TrackNewLine();
            Console.WriteLine();

            return this;
        }
    }

    private void TrackNewLine()
    {
        //foreach (var length in _previousLineLengths)
        //    Write($"{length},".InDarkCyan());

        foreach (var line in _appendableLines)
            line.Offset++;

        _previousLineLengths.Push(_currentLineLength);
        _currentLineLength = 0;
    }

    public IConsole ClearLine()
    {
        lock (locker)
        {
            var wrappedLines = new List<int>();
            var chunk = _currentLineLength;
            while (chunk >= Console.BufferWidth)
            {
                wrappedLines.Add(Console.BufferWidth - 1);
                chunk -= Console.BufferWidth;
            }
            var clearText = "".PadLeft(Console.BufferWidth, ' ');

            foreach (var line in wrappedLines)
            {
                var startW = Console.GetCursorPosition();
                Console.SetCursorPosition(0, startW.Top);
                Console.Write(clearText);
                Console.SetCursorPosition(0, startW.Top - 1);
            }

            var start = Console.GetCursorPosition();
            Console.SetCursorPosition(0, start.Top);
            Console.Write(clearText);
            Console.SetCursorPosition(0, start.Top);
            _currentLineLength = 0;
            return this;
        }
    }

    //public IConsole ClearLines(int numberToClear)
    //{
    //    lock (locker)
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
    //}

    private ConsoleKeyInfo ReadKey()
    {
        lock (locker)
        {
            ClearWaitingKeys();
            var start = Console.GetCursorPosition();
            var retval = Console.ReadKey();
            var end = Console.GetCursorPosition();
            if (end.Left > start.Left)
                _currentLineLength += (end.Left - start.Left);

            if (retval.Key == ConsoleKey.Enter)
            {
                //_previousLineLengths.Push(_currentLineLength);
                //_currentLineLength = 0;
                //TrackNewLine();
            }

            // Escape borks the console briefly. The first printable character in a string to be displayed, won't be shown.
            // If you try to just print some whitespace, that won't fix it, it needs to be something significant.
            // And not having a space printed first works ok in debug mode (old console app maybe) but
            // hangs Windows Terminal !!!! :(
            if (retval.Key == ConsoleKey.Escape)
                DoWrite(" X");

            return retval;
        }
    }

    void ClearWaitingKeys()
    {
        lock (locker)
        {
            var current = Console.GetCursorPosition();
            while (Console.KeyAvailable)
            {
                Console.ReadKey();
                // Reset position after each read, so we won't get a load of returns that could scroll the screen or sommat...
                Console.SetCursorPosition(current.Left, current.Top);
            }
            // Overwrite any lingering character
            Console.Write(" ");
            Console.SetCursorPosition(current.Left, current.Top);
        }
    }

    public string? ReadLine()
    {
        lock (locker)
        {
            ClearWaitingKeys();

            var retval = Console.ReadLine();
            _currentLineLength += retval!.Length;

            ResetCursorPosition(true);

            return retval;
        }
    }

    private void ResetCursorPosition(bool afterEnter = false)
    {
        var remainder = _currentLineLength % Console.BufferWidth;
        var current = Console.GetCursorPosition();
        if (afterEnter)
            Console.SetCursorPosition(remainder, current.Top - 1);
        else
            Console.SetCursorPosition(remainder, current.Top);
    }

    public IConsole.IPrompt StartPrompt(params object[] items)
    {
        return new PromptImplementation(this, items);
    }

    public void Abort(params object[] items)
    {
        if (items is not null && items.Length == 1 && items[0] is not Coloured)
            WriteLine(items[0].ToString().InRed());
        else
            WriteLine(items);
        Environment.Exit(1);
    }

    public IConsole.IAppendableLine StartAppendable(params object[] items)
    {
        lock (locker)
        {
            if (_currentLineLength > 0)
                WriteLine();

            WriteLine(items);
            var length = _previousLineLengths.Peek();

            var appendable = new AppendableLineImplementation(this, length);
            _appendableLines.Add(appendable);
            return appendable;
        }
    }

    private void AppendableWrite(AppendableLineImplementation line, params object[] items)
    {
        lock (locker)
        {
            var originalCursor = Console.GetCursorPosition();
            var originalLength = _currentLineLength;

            Console.SetCursorPosition(line.Length, originalCursor.Top - line.Offset);
            _currentLineLength = line.Length;

            Write(items);

            line.Length = _currentLineLength;

            _currentLineLength = originalLength;
            Console.SetCursorPosition(originalCursor.Left, originalCursor.Top);
        }
    }

    private HashSet<AppendableLineImplementation> _appendableLines = new();

    private class AppendableLineImplementation : IConsole.IAppendableLine
    {
        private static object locker = new object();
        private readonly VConsole console;
        //private readonly char[] frames = new[] { '|', '/', '-', '\\' };
        //private Task spinTask;
        private Spinner spinner;

        public int Offset = 1;
        public int Length = 0;
        //private bool spinning = false;

        public AppendableLineImplementation(VConsole console, int length)
        {
            Length = length;
            this.console = console;
        }

        public IConsole.IAppendableLine StartSpinner()
        {
            //spinning = true;
            //console.AppendableWrite(this, frames[3]);
            //spinTask = DoSpinner();
            spinner = new Spinner(this);

            return this;
        }

        private class Spinner
        {
            private readonly char[] frames = new[] { '|', '/', '-', '\\' };
            private readonly Colour[] colours = new[] { Colour.Red, Colour.DarkRed, Colour.Magenta, Colour.DarkMagenta, Colour.DarkBlue, Colour.Blue, Colour.DarkCyan, Colour.Cyan };
            private readonly AppendableLineImplementation appender;
            private bool spinning = true;


            public Spinner(AppendableLineImplementation appender)
            {
                this.appender = appender;
                appender.console.AppendableWrite(appender, frames[0]);
                DoSpinner();
            }
            public void Stop() => spinning = false;

            public async Task DoSpinner()
            {
                var frame = 0;
                var colour = 0;
                var colourStep = -1;
                //var colour = Colour.Black;
                while (spinning)
                {
                    lock (locker)
                    {
                        if (!spinning) return;
                        appender.Length--;
                        appender.console.AppendableWrite(appender, frames[frame].InColour(colours[colour]));
                    }
                    frame++;
                    if (frame == 4) frame = 0;

                    if (colour == 7) colourStep = 0 - colourStep;
                    if (colour == 0) colourStep = 0 - colourStep;
                    colour += colourStep;

                    await Task.Delay(100);
                }
            }
        }

        public IConsole.IAppendableLine Write(params object[] items)
        {
            //spinning = false;
            lock (locker)
            {
                if (spinner is not null)
                {
                    Length--;
                    spinner.Stop();
                    spinner = null;
                }
                console.AppendableWrite(this, items);
            }
            return this;
        }
    }


    private class NullWriter : IConsole
    {
        public IConsole Verbose => throw new NotImplementedException();

        public void Abort(params object[] items)
        {
        }

        public IConsole ClearLine()
        {
            return this;
        }

        public IConsole.IAppendableLine StartAppendable(params object[] items)
        {
            throw new NotImplementedException("Do I want a verbose appendable? Maybe not. Either lose this in the split, or implement a null version too...");
        }

        public IConsole.IPrompt StartPrompt(params object[] items)
        {
            throw new NotImplementedException("Do I want a verbose prompt? Maybe not. Either lose this in the split, or implement a null version too...");
        }

        public IConsole Write(params object[]? items)
        {
            return this;
        }

        public IConsole WriteLine(params object[]? items)
        {
            return this;
        }

        public IConsole WriteLine()
        {
            return this;
        }
    }
}

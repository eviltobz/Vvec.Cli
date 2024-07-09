namespace Vvec.Cli.UI;

public partial class VConsole : IConsole, IInternalConsole
{
    #region Singleton instantiation
    // VConsole needs to be a singleton so it can track what it has done if callers need to change previous output
    private VConsole() { }

    private static readonly IConsole _instance = new VConsole();

    public static IConsole Instance => _instance;

    public static TestConsole CreateTestConsole(Action<string> assertFail) =>
        new TestConsole((console, items) => new PromptImplementation(console), assertFail);

    public IConsole Verbose { get; private set; } = new NullWriter();

    public static void SetVerbose()
    {
        ((VConsole)_instance).Verbose = _instance;
    }
    #endregion

    private static object locker = new object();
    private readonly Stack<int> _previousLineLengths = new Stack<int>();

    private int _currentLineLength;
    private HashSet<AppendableLineImplementation> _appendableLines = new();

    IInternalConsole DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true)
    {
        return (this as IInternalConsole).DoWrite(text, foregroundColour, backgroundColour, updateCurrentInfo);
    }

    IInternalConsole IInternalConsole.DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true)
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
            TrackNewLine();
            Console.WriteLine();

            return this;
        }
    }

    private void TrackNewLine()
    {
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

    ConsoleKeyInfo IInternalConsole.ReadKey()
    {
        lock (locker)
        {
            ClearWaitingKeys();
            var start = Console.GetCursorPosition();
            var retval = Console.ReadKey();
            var end = Console.GetCursorPosition();
            if (end.Left > start.Left)
                _currentLineLength += (end.Left - start.Left);

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

    string? IInternalConsole.ReadLine()
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

    private class AppendableLineImplementation : IConsole.IAppendableLine
    {
        private static object locker = new object();
        private readonly VConsole console;
        private Spinner spinner;

        public int Offset = 1;
        public int Length = 0;

        public AppendableLineImplementation(VConsole console, int length)
        {
            Length = length;
            this.console = console;
        }

        public IConsole.IAppendableLine StartSpinner()
        {
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


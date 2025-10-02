using System.Runtime.CompilerServices;
using System.Text;

namespace Vvec.Cli.UI;

internal static class StringExtensions
{
    public static int DisplayLength(this string? text)
    {
        if (text is null)
            return 0;

        var len = 0;
        var isAnsiEscape = false;
        foreach(char chr in text)
        {
            if(chr == AnsiCode.Escape1)
            {
                isAnsiEscape = true;
                continue;
            }
            if (isAnsiEscape && chr != AnsiCode.Terminator)
                continue;
            if (isAnsiEscape && chr == AnsiCode.Terminator)
            {
                isAnsiEscape = false;
                continue;
            }

            len++;
        }
        return len;
    }

    public static bool ContainsAnsiFormatting(this object[] items)
    {
        if (items is null)
            return false;

        foreach (var item in items)
        {
            if (item is not null)
                foreach (char chr in item.ToString())
                {
                    if (chr == AnsiCode.Escape1)
                    {
                        return true;
                    }
                }
        }
        return false;
    }

}

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

    //IInternalConsole DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true)
    //{
    //    return (this as IInternalConsole).DoWrite(text, foregroundColour, backgroundColour, updateCurrentInfo);
    //}

    //IInternalConsole IInternalConsole.DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true)
    //{
    //    lock (locker)
    //    {
    //        ConsoleColor originalForeground = default;
    //        ConsoleColor originalBackground = default;

    //        if (foregroundColour.HasValue)
    //        {
    //            originalForeground = Console.ForegroundColor;
    //            Console.ForegroundColor = (ConsoleColor)foregroundColour.Value;
    //        }
    //        if (backgroundColour.HasValue)
    //        {
    //            originalBackground = Console.BackgroundColor;
    //            Console.BackgroundColor = (ConsoleColor)backgroundColour.Value;
    //        }

    //        Console.Write(text);

    //        if (foregroundColour.HasValue)
    //            Console.ForegroundColor = originalForeground;
    //        if (backgroundColour.HasValue)
    //            Console.BackgroundColor = originalBackground;


    //        if (updateCurrentInfo)
    //            _currentLineLength += text.DisplayLength();
    //        return this;
    //    }
    //}

    IInternalConsole IInternalConsole.DoWrite(object[]? items, bool updateCurrentInfo = true)
        => (IInternalConsole)DoWrite2(items, updateCurrentInfo);

    //IInternalConsole IInternalConsole.DoWrite2(object[]? items, bool updateCurrentInfo = true)
    public IConsole DoWrite2(object[]? items, bool updateCurrentInfo = true)
    {
        lock (locker)
        {
            var len = 0;
            if (items is not null)
            {
                //foreach (var item in items)
                //{
                //    if (item is Coloured coloured)
                //        DoWrite(coloured.Value, coloured.Foreground, coloured.Background);
                //    else
                //        DoWrite(item?.ToString());
                //}
                var sb = new StringBuilder();
                foreach (var item in items)
                {
                    if (item is null)
                        continue;

                    var itemString = item.ToString();
                    sb.Append(itemString);
                    if (item is not AnsiCode)
                        len += itemString.DisplayLength();
                }
                sb.Append(Style.Default);
                Console.Write(sb.ToString());
            }

            if (updateCurrentInfo)
                //_currentLineLength += text is null ? 0 : text.Length;
                _currentLineLength += len;
        }
        return this;
    }

    private const bool UseNewWrite = true;
    public IConsole Write(params object[]? items)
    //=> DoWrite(items);
    {

        if (UseNewWrite)
            return DoWrite2(items);

        //if (items is not null)
        //    foreach (var item in items)
        //        if (item is Coloured coloured)
        //            DoWrite(coloured.Value, coloured.Foreground, coloured.Background);
        //        else
        //            DoWrite(item?.ToString());

        return this;
    }



    public IConsole WriteLine(params object[]? items)
    {
        lock (locker)
        {
            return Write(items).WriteLine();
        }
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
                DoWrite2([" X"]);

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

    public void AbortApplication(params object[] items)
    {
        //if (items is not null && items.Length == 1 && items[0] is not Coloured)
        //    WriteLine(items[0].ToString().InRed());
        if (items is not null && !items.ContainsAnsiFormatting())
            WriteLine([FG.Red, .. items]);

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

    private class AppendableLineImplementation : IConsole.IAppendableLine, IConsole.IStatus
    {
        private static object locker = new object();
        private readonly VConsole console;
        private Spinner spinner;
        private Ellipsis ellipsis;

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

        public IConsole.IAppendableLine StartEllipsis()
        {
            ellipsis = new Ellipsis(this);
            return this;
        }

        private class Spinner
        {
            private readonly char[] frames = new[] { '|', '/', '-', '\\' };
            //private readonly Colour[] OLDcolours = new[] { Colour.Red, Colour.DarkRed, Colour.Magenta, Colour.DarkMagenta, Colour.DarkBlue, Colour.Blue, Colour.DarkCyan, Colour.Cyan };
            private readonly AnsiCode[] colours = new[] { FG.Red, FG.DarkRed, FG.Magenta, FG.DarkMagenta, FG.DarkBlue, FG.Blue, FG.DarkCyan, FG.Cyan };
            private readonly AppendableLineImplementation appender;
            private bool spinning = true;

            public Spinner(AppendableLineImplementation appender)
            {
                this.appender = appender;
                appender.console.AppendableWrite(appender, frames[0]);
                //HACKY_SPINNER_LENGTH = 1;
                DoSpinner();
            }

            public void Stop()
            {
                appender.Length--;
                //appender.Length -= HACKY_SPINNER_LENGTH;
                appender.console.AppendableWrite(appender, " ");
                appender.Length--;
                spinning = false;
            }

            //private int HACKY_SPINNER_LENGTH = 0;
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
                        //appender.Length -= HACKY_SPINNER_LENGTH;
                        //appender.console.AppendableWrite(appender, frames[frame].InColour(OLDcolours[colour]));
                        appender.console.AppendableWrite(appender, colours[colour], frames[frame]);
                        //HACKY_SPINNER_LENGTH = colours[colour].ToString().Length + 1;
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

        private class Ellipsis
        {
            private readonly AppendableLineImplementation appender;
            private bool spinning = true;
            private readonly int startPos;

            public Ellipsis(AppendableLineImplementation appender)
            {
                this.appender = appender;
                startPos = appender.Length;
                DoEllipsis();
            }

            public void Stop()
            {
                spinning = false;
                appender.Length = startPos;
                appender.console.AppendableWrite(appender, "   ");
                appender.Length = startPos;
            }

            public async Task DoEllipsis()
            {
                var frame = 0;
                var state = new char[3];

                while (spinning)
                {
                    state[0] = frame < 1 ? ' ' : '.';
                    state[1] = frame < 2 ? ' ' : '.';
                    state[2] = frame < 3 ? ' ' : '.';

                    lock (locker)
                    {
                        if (!spinning) return;
                        appender.Length = startPos;
                        appender.console.AppendableWrite(appender, new string(state));
                    }

                    frame++;
                    if (frame == 4) frame = 0;

                    await Task.Delay(200);
                }
            }
        }


        public IConsole.IAppendableLine Write(params object[] items)
        {
            lock (locker)
            {
                StopSpinner();
                StopEllipsis();
                console.AppendableWrite(this, items);
            }
            return this;
        }

        private void StopSpinner()
        {
            if (spinner is not null)
            {
                spinner.Stop();
                spinner = null;
            }
        }
        private void StopEllipsis()
        {
            if (ellipsis is not null)
            {
                ellipsis.Stop();
                ellipsis = null;
            }
        }

        private int statusPosition = 0;
        public IConsole.IStatus StartStatus(params object[] items)
        {
            lock (locker)
            {
                statusPosition = Length;
                console.AppendableWrite(this, items);
            }
            return this;
        }

        public IConsole.IStatus Update(params object[] items)
        {
            lock (locker)
            {
                StopSpinner();
                StopEllipsis();
                var oldLength = Length;
                Length = statusPosition;
                StartStatus(items);
                var newLength = Length;
                var blanking = oldLength - Length;
                if (blanking > 0)
                    console.AppendableWrite(this, new string(' ', blanking));
                //for (int i = Length; i < oldLength; i++)
                //    console.AppendableWrite(this, " ");
                Length = newLength;
            }
            return this;
        }

        public IConsole.IStatus WithSpinner()
        {
            StartSpinner();
            return this;
        }
        public IConsole.IStatus WithEllipsis()
        {
            StartEllipsis();
            return this;
        }

        public IConsole.IAppendableLine Finish()
        {
            StopEllipsis();
            StopSpinner();
            return this;
        }

        public IConsole.IAppendableLine Finish(params object[] items)
        {
            StopEllipsis();
            StopSpinner();
            return Update(items) as IConsole.IAppendableLine;
        }
    }

    private class NullWriter : IConsole
    {
        public IConsole Verbose => throw new NotImplementedException();

        public void AbortApplication(params object[] items)
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


using Microsoft.CodeAnalysis.Operations;

namespace Vvec.Cli.UI;

//internal static class UiExtensions
//{
//    public static Coloured InBlack(this object? value) => new Coloured(value, Colour.Black);
//    public static Coloured InDarkBlue(this object? value) => new Coloured(value, Colour.DarkBlue);
//    public static Coloured InDarkGreen(this object? value) => new Coloured(value, Colour.DarkGreen);
//    public static Coloured InDarkCyan(this object? value) => new Coloured(value, Colour.DarkCyan);
//    public static Coloured InDarkRed(this object? value) => new Coloured(value, Colour.DarkRed);
//    public static Coloured InDarkMagenta(this object? value) => new Coloured(value, Colour.DarkMagenta);
//    public static Coloured InDarkYellow(this object? value) => new Coloured(value, Colour.DarkYellow);
//    public static Coloured InDarkGrey(this object? value) => new Coloured(value, Colour.DarkGrey);
//    public static Coloured InGrey(this object? value) => new Coloured(value, Colour.Grey);
//    public static Coloured InBlue(this object? value) => new Coloured(value, Colour.Blue);
//    public static Coloured InGreen(this object? value) => new Coloured(value, Colour.Green);
//    public static Coloured InCyan(this object? value) => new Coloured(value, Colour.Cyan);
//    public static Coloured InRed(this object? value) => new Coloured(value, Colour.Red);
//    public static Coloured InMagenta(this object? value) => new Coloured(value, Colour.Magenta);
//    public static Coloured InYellow(this object? value) => new Coloured(value, Colour.Yellow);
//    public static Coloured InWhite(this object? value) => new Coloured(value, Colour.White);
//    public static Coloured InColour(this object? value, Colour colour) => new Coloured(value, colour);

//    public static Coloured OnBlack(this object? value) => new Coloured(value, null, Colour.Black);
//    public static Coloured OnDarkBlue(this object? value) => new Coloured(value, null, Colour.DarkBlue);
//    public static Coloured OnDarkGreen(this object? value) => new Coloured(value, null, Colour.DarkGreen);
//    public static Coloured OnDarkCyan(this object? value) => new Coloured(value, null, Colour.DarkCyan);
//    public static Coloured OnDarkRed(this object? value) => new Coloured(value, null, Colour.DarkRed);
//    public static Coloured OnDarkMagenta(this object? value) => new Coloured(value, null, Colour.DarkMagenta);
//    public static Coloured OnDarkYellow(this object? value) => new Coloured(value, null, Colour.DarkYellow);
//    public static Coloured OnDarkGrey(this object? value) => new Coloured(value, null, Colour.DarkGrey);
//    public static Coloured OnGrey(this object? value) => new Coloured(value, null, Colour.Grey);
//    public static Coloured OnBlue(this object? value) => new Coloured(value, null, Colour.Blue);
//    public static Coloured OnGreen(this object? value) => new Coloured(value, null, Colour.Green);
//    public static Coloured OnCyan(this object? value) => new Coloured(value, null, Colour.Cyan);
//    public static Coloured OnRed(this object? value) => new Coloured(value, null, Colour.Red);
//    public static Coloured OnMagenta(this object? value) => new Coloured(value, null, Colour.Magenta);
//    public static Coloured OnYellow(this object? value) => new Coloured(value, null, Colour.Yellow);
//    public static Coloured OnWhite(this object? value) => new Coloured(value, null, Colour.White);

//    public static Coloured OnColour(this object? value, Colour colour) => new Coloured(value, null, colour);
//}

//public enum Colour
//{
//    Black = ConsoleColor.Black,
//    DarkBlue = ConsoleColor.DarkBlue,
//    DarkGreen = ConsoleColor.DarkGreen,
//    DarkCyan = ConsoleColor.DarkCyan,
//    DarkRed = ConsoleColor.DarkRed,
//    DarkMagenta = ConsoleColor.DarkMagenta,
//    DarkYellow = ConsoleColor.DarkYellow,
//    DarkGrey = ConsoleColor.DarkGray,
//    Grey = ConsoleColor.Gray,
//    Blue = ConsoleColor.Blue,
//    Green = ConsoleColor.Green,
//    Cyan = ConsoleColor.Cyan,
//    Red = ConsoleColor.Red,
//    Magenta = ConsoleColor.Magenta,
//    Yellow = ConsoleColor.Yellow,
//    White = ConsoleColor.White,
//}



// ANSI code reference stuff at: https://ss64.com/nt/syntax-ansi.html
public class AnsiCode//(int code)
{
    internal const char Escape1 = (char)27;
    internal const char Escape2 = '[';
    internal const char Terminator = 'm';
    private static readonly string EscapeSequence = new string([Escape1, Escape2]);
    private readonly string sequence;// = Escape + code.ToString() + "m";

    public override string ToString()
    {
        return sequence;
    }

    public AnsiCode(int code)
    {
        sequence = EscapeSequence + code.ToString() + "m";
    }

    private AnsiCode(string validSequence)
    {
        sequence = validSequence;
    }

    public static AnsiCode operator +(AnsiCode left, AnsiCode right)
        => new AnsiCode(left.ToString() + right.ToString());
}

public class FG
{
    public static readonly AnsiCode Black = new(30);
    public static readonly AnsiCode DarkRed = new(31);
    public static readonly AnsiCode DarkGreen = new(32);
    public static readonly AnsiCode DarkYellow = new(33);
    public static readonly AnsiCode DarkBlue = new(34);
    public static readonly AnsiCode DarkMagenta = new(35);
    public static readonly AnsiCode DarkCyan = new(36);
    public static readonly AnsiCode DarkGrey = new(90);
    public static readonly AnsiCode Grey = new(37);
    public static readonly AnsiCode Red = new(91);
    public static readonly AnsiCode Green = new(92);
    public static readonly AnsiCode Yellow = new(93);
    public static readonly AnsiCode Blue = new(94);
    public static readonly AnsiCode Magenta = new(95);
    public static readonly AnsiCode Cyan = new(96);
    public static readonly AnsiCode White = new(97);

    public static readonly AnsiCode Default = Grey;
}

public class BG
{
    public static readonly AnsiCode Black = new(40);
    public static readonly AnsiCode DarkRed = new(41);
    public static readonly AnsiCode DarkGreen = new(42);
    public static readonly AnsiCode DarkYellow = new(43);
    public static readonly AnsiCode DarkBlue = new(44);
    public static readonly AnsiCode DarkMagenta = new(45);
    public static readonly AnsiCode DarkCyan = new(46);
    public static readonly AnsiCode DarkGrey = new(100);
    public static readonly AnsiCode Grey = new(47);
    public static readonly AnsiCode Red = new(101);
    public static readonly AnsiCode Green = new(102);
    public static readonly AnsiCode Yellow = new(103);
    public static readonly AnsiCode Blue = new(104);
    public static readonly AnsiCode Magenta = new(105);
    public static readonly AnsiCode Cyan = new(106);
    public static readonly AnsiCode White = new(107);
}

public class Style
{
    public static readonly AnsiCode Default = new(0);
    public static readonly AnsiCode Underline = new(4);
    public static readonly AnsiCode NoUnderline = new(24);

    // Bold (in windows terminal at least) is just white.
    // public static readonly AnsiCode Bold = new(1);

    // Not entirely sure WTF this is meant to do.
    //public static readonly AnsiCode Reverse = new(7);
    //public static readonly AnsiCode UnReverse = new(27);

    public static string As(AnsiCode format, params object[] content)
        => string.Concat([format.ToString(), ..content.Select(s => s.ToString())]);

    //public static object[] As(AnsiCode format, params object[] content)
    //    => [format, .. content, Style.Default];
}

/*


Colour	Foreground	Background	COLOR equivalent
Default	Esc[0m
Black BLACK █	Esc[30m	Esc[40m	0
Red DARK_RED █	Esc[31m	Esc[41m	4
Green DARK_GREEN █	Esc[32m	Esc[42m	2
Yellow DARK_YELLOW █	Esc[33m	Esc[43m	6
Blue DARK_BLUE █	Esc[34m	Esc[44m	1
Magenta DARK_MAGENTA █	Esc[35m	Esc[45m	5
Cyan DARK_CYAN █	Esc[36m	Esc[46m	3
Light gray DARK_WHITE █	Esc[37m	Esc[47m	7
Dark gray BRIGHT_BLACK █	Esc[90m	Esc[100m	8
Light red BRIGHT_RED █	Esc[91m	Esc[101m	C
Light green BRIGHT_GREEN █	Esc[92m	Esc[102m	A
Light yellow BRIGHT_YELLOW █	Esc[93m	Esc[103m	E
Light blue BRIGHT_BLUE █	Esc[94m	Esc[104m	9
Light magenta BRIGHT_MAGENTA █	Esc[95m	Esc[105m	D
Light cyan BRIGHT_CYAN █	Esc[96m	Esc[106m	B
White WHITE █	Esc[97m	Esc[107m	F
Bold	Esc[1m
Underline	Esc[4m
No underline	Esc[24m
Reverse text	Esc[7m
Positive text (not reversed)	Esc[27m


*/

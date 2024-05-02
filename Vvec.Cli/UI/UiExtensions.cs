namespace Vvec.Cli.UI
{
    public static class UiExtensions
    {
        public static Coloured InBlack(this object? value) => new Coloured(value, Colour.Black);
        public static Coloured InDarkBlue(this object? value) => new Coloured(value, Colour.DarkBlue);
        public static Coloured InDarkGreen(this object? value) => new Coloured(value, Colour.DarkGreen);
        public static Coloured InDarkCyan(this object? value) => new Coloured(value, Colour.DarkCyan);
        public static Coloured InDarkRed(this object? value) => new Coloured(value, Colour.DarkRed);
        public static Coloured InDarkMagenta(this object? value) => new Coloured(value, Colour.DarkMagenta);
        public static Coloured InDarkYellow(this object? value) => new Coloured(value, Colour.DarkYellow);
        public static Coloured InDarkGrey(this object? value) => new Coloured(value, Colour.DarkGrey);
        public static Coloured InGrey(this object? value) => new Coloured(value, Colour.Grey);
        public static Coloured InBlue(this object? value) => new Coloured(value, Colour.Blue);
        public static Coloured InGreen(this object? value) => new Coloured(value, Colour.Green);
        public static Coloured InCyan(this object? value) => new Coloured(value, Colour.Cyan);
        public static Coloured InRed(this object? value) => new Coloured(value, Colour.Red);
        public static Coloured InMagenta(this object? value) => new Coloured(value, Colour.Magenta);
        public static Coloured InYellow(this object? value) => new Coloured(value, Colour.Yellow);
        public static Coloured InWhite(this object? value) => new Coloured(value, Colour.White);
        public static Coloured InColour(this object? value, Colour colour) => new Coloured(value, colour);

        public static Coloured OnBlack(this object? value) => new Coloured(value, null, Colour.Black);
        public static Coloured OnDarkBlue(this object? value) => new Coloured(value, null, Colour.DarkBlue);
        public static Coloured OnDarkGreen(this object? value) => new Coloured(value, null, Colour.DarkGreen);
        public static Coloured OnDarkCyan(this object? value) => new Coloured(value, null, Colour.DarkCyan);
        public static Coloured OnDarkRed(this object? value) => new Coloured(value, null, Colour.DarkRed);
        public static Coloured OnDarkMagenta(this object? value) => new Coloured(value, null, Colour.DarkMagenta);
        public static Coloured OnDarkYellow(this object? value) => new Coloured(value, null, Colour.DarkYellow);
        public static Coloured OnDarkGrey(this object? value) => new Coloured(value, null, Colour.DarkGrey);
        public static Coloured OnGrey(this object? value) => new Coloured(value, null, Colour.Grey);
        public static Coloured OnBlue(this object? value) => new Coloured(value, null, Colour.Blue);
        public static Coloured OnGreen(this object? value) => new Coloured(value, null, Colour.Green);
        public static Coloured OnCyan(this object? value) => new Coloured(value, null, Colour.Cyan);
        public static Coloured OnRed(this object? value) => new Coloured(value, null, Colour.Red);
        public static Coloured OnMagenta(this object? value) => new Coloured(value, null, Colour.Magenta);
        public static Coloured OnYellow(this object? value) => new Coloured(value, null, Colour.Yellow);
        public static Coloured OnWhite(this object? value) => new Coloured(value, null, Colour.White);

        public static Coloured OnColour(this object? value, Colour colour) => new Coloured(value, null, colour);
    }

    public enum Colour
    {
        Black = ConsoleColor.Black,
        DarkBlue = ConsoleColor.DarkBlue,
        DarkGreen = ConsoleColor.DarkGreen,
        DarkCyan = ConsoleColor.DarkCyan,
        DarkRed = ConsoleColor.DarkRed,
        DarkMagenta = ConsoleColor.DarkMagenta,
        DarkYellow = ConsoleColor.DarkYellow,
        DarkGrey = ConsoleColor.DarkGray,
        Grey = ConsoleColor.Gray,
        Blue = ConsoleColor.Blue,
        Green = ConsoleColor.Green,
        Cyan = ConsoleColor.Cyan,
        Red = ConsoleColor.Red,
        Magenta = ConsoleColor.Magenta,
        Yellow = ConsoleColor.Yellow,
        White = ConsoleColor.White,
    }
}

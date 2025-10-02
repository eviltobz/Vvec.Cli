using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class ShowColoursCommand : ISubCommand
{
    private readonly IConsole cons;

    public ShowColoursCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public static string Name => "colours";

    public static string Description => "Debuggy method to show how the colours are displayed";

    public void Execute()
    {
        //cons.WriteLine("The colours are ordered be as close to a spectrum as I can see:")
        //    .WriteLine("*** Black".InBlack())
        //    .WriteLine("*** DarkGrey".InDarkGrey())
        //    .WriteLine("*** Grey".InGrey())
        //    .WriteLine("*** White".InWhite())

        //    .WriteLine("*** Red".InRed())
        //    .WriteLine("*** DarkRed".InDarkRed())
        //    .WriteLine("*** Magenta".InMagenta())
        //    .WriteLine("*** DarkMagenta".InDarkMagenta())

        //    .WriteLine("*** DarkBlue".InDarkBlue())
        //    .WriteLine("*** Blue".InBlue())
        //    .WriteLine("*** DarkCyan".InDarkCyan())
        //    .WriteLine("*** Cyan".InCyan())

        //    .WriteLine("*** Yellow".InYellow())
        //    .WriteLine("*** DarkYellow".InDarkYellow())
        //    .WriteLine("*** DarkGreen".InDarkGreen())
        //    .WriteLine("*** Green".InGreen())
        //    ;
        //cons.WriteLine(Style.Underline, FG.Red, BG.Cyan, "And now with ANSI flavouring...", Style.Default);

        cons.WriteLine(FG.Default, "The colours are ordered be as close to a spectrum as I can see:")
            .WriteLine(FG.Black, "*** Black")
            .WriteLine(FG.DarkGrey, "*** DarkGrey")
            .WriteLine(FG.Grey, "*** Grey")
            .WriteLine(FG.White, "*** White")

            .WriteLine(FG.Red, "*** Red")
            .WriteLine(FG.DarkRed, "*** DarkRed")
            .WriteLine(FG.Magenta, "*** Magenta")
            .WriteLine(FG.DarkMagenta, "*** DarkMagenta")

            .WriteLine(FG.DarkBlue, "*** DarkBlue")
            .WriteLine(FG.Blue, "*** Blue")
            .WriteLine(FG.DarkCyan, "*** DarkCyan")
            .WriteLine(FG.Cyan, "*** Cyan")

            .WriteLine(FG.Yellow, "*** Yellow")
            .WriteLine(FG.DarkYellow, "*** DarkYellow")
            .WriteLine(FG.DarkGreen, "*** DarkGreen")
            .WriteLine(FG.Green, "*** Green")
            .Write(Style.Default)
            ;
    }
}

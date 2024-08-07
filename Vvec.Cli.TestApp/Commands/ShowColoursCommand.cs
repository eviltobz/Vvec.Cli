﻿using Vvec.Cli.Arguments;
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
        cons.WriteLine("The colours are ordered be as close to a spectrum as I can see:")
            .WriteLine("*** Black".InBlack())
            .WriteLine("*** DarkGrey".InDarkGrey())
            .WriteLine("*** Grey".InGrey())
            .WriteLine("*** White".InWhite())

            .WriteLine("*** Red".InRed())
            .WriteLine("*** DarkRed".InDarkRed())
            .WriteLine("*** Magenta".InMagenta())
            .WriteLine("*** DarkMagenta".InDarkMagenta())

            .WriteLine("*** DarkBlue".InDarkBlue())
            .WriteLine("*** Blue".InBlue())
            .WriteLine("*** DarkCyan".InDarkCyan())
            .WriteLine("*** Cyan".InCyan())

            .WriteLine("*** Yellow".InYellow())
            .WriteLine("*** DarkYellow".InDarkYellow())
            .WriteLine("*** DarkGreen".InDarkGreen())
            .WriteLine("*** Green".InGreen())
            ;
    }
}

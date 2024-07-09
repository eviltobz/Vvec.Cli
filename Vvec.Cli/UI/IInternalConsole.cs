namespace Vvec.Cli.UI;

internal interface IInternalConsole : IConsole
{
    string? ReadLine();

    ConsoleKeyInfo ReadKey();

    IInternalConsole DoWrite(string? text, Colour? foregroundColour = null, Colour? backgroundColour = null, bool updateCurrentInfo = true);
}


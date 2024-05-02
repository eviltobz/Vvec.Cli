//using System.CommandLine;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

public class NoDescriptionCommand : ISubCommand
{
    private readonly IConsole cons;

    public static string Name => "NoDescription";

    public static string Description => null;

    public NoDescriptionCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public void Execute()
    {
        cons.WriteLine("I have no description");
    }
}

public class FurtleCommand : ISubCommandAsync
{
    //private readonly FurtleProcess furtler;

    //public FurtleCommand(FurtleProcess furtler)
    //{
    //    this.furtler = furtler;
    //}

    public static string Name => "furtle";

    public static string Description => "Hacky method for dev testing. To aid testing the help output, this command has a rather long description, so the arg parser console can try to do some nice formatting when a line wraps.";

    //public async Task Execute() =>
    //    await furtler.Execute();
//}

//public class FurtleProcess
//{
    private readonly IConsole cons;

    //public FurtleProcess(IConsole cons)
    public FurtleCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public async Task Execute()
    {
        cons.WriteLine().WriteLine("   *** Hacky method for furtling with stuff ***".InDarkYellow());

        //var dom = System.Threading.Thread.GetDomain();
        //cons
        //    .WriteLine("Environment.ProcessPath:".PadRight(35), Environment.ProcessPath.InYellow())
        //    .WriteLine("Directory.GetCurrentDirectory:".PadRight(35), Directory.GetCurrentDirectory().InYellow())
        //    .WriteLine("AppDomain.BaseDirectory:".PadRight(35), dom.BaseDirectory.InYellow())
        //    .WriteLine("AppDomain.DynamicDirectory:".PadRight(35), dom.DynamicDirectory.InYellow())
        //    ;

        //cons.WriteLine(Path.GetFullPath(Environment.ProcessPath)).WriteLine(Path.GetDirectoryName(Environment.ProcessPath));

        var bob = cons.StartPrompt("Here's a bunch of random text. tsra tsrnei etsran eitsr itsra neit teitsr. Here's another bunch of random text. tsra tsrnei etsran eitsr itsra neit teitsr:")
            .GetFreeTextOrDefault("<default value>");
        cons.WriteLine("Got:", bob.InBlue());

        cons.WriteLine().WriteLine("I should split out several, better named commands from the unreachable code below to check different aspects of the UI stuff...".InDarkRed().OnDarkYellow());

        return;
        var bufH = System.Console.BufferHeight;
        var bufW = System.Console.BufferWidth;

        var winH = Console.WindowHeight;
        var winW = Console.WindowWidth;
        var winT = Console.WindowTop;
        var winL = Console.WindowLeft;

        var largeH = Console.LargestWindowHeight;
        var largeW = Console.LargestWindowWidth;

        var (curL, curT) = Console.GetCursorPosition();

        cons.WriteLine("Buf:", bufW, "x", bufH, ", Win:", winW, "x", winH, ", Pos:", winL, ":", winT, ", Largest:", largeH, "x", largeW, ", Cur:", curL, "x", curT);


        return;


        var app1 = cons.StartAppendable("Appendable1:").StartSpinner();
        var app2 = cons.StartAppendable("Second appendable").StartSpinner();
        await Task.Delay(2000).ConfigureAwait(true);
        cons.WriteLine("1 second");
        await Task.Delay(1000);
        cons.WriteLine("2 seconds");
        app1.Write(" and den").StartSpinner();
        await Task.Delay(1000);
        cons.WriteLine("3 seconds");
        await Task.Delay(1000);
        cons.WriteLine("4 seconds");
        var defaulty = cons.StartPrompt("Text input with a default value (which isn't shown automagically, you need to add that...)").GetFreeTextOrDefault("your mum");
        cons.WriteLine("Got: ", defaulty.InDarkYellow());
        app1.Write("done");
        app2.Write("done");

        //var app1 = cons.StartAppendable("My ", "1st".InDarkYellow(), " appendable.");
        //var app2 = cons.StartAppendable("My ", "2nd".InDarkYellow(), " appendable.");
        //var app3 = cons.StartAppendable("My ", "3rd".InDarkYellow(), " appendable.");
        //cons.Write("text").Write("text".InRed()).Write("meh").Write("bork".InGreen());
        //cons.WriteLine();
        //cons.Write("an", "array".InMagenta(), "of".InDarkBlue(), "text".InCyan()).WriteLine();
        //cons.WriteLine("some ", "really highlighted ".InRed().OnYellow(), "text");
        //var a = 1;
        //var b = "meh";
        //cons.Write($"some interpolated text: {a}".InRed(), $", {b}".InGreen()).WriteLine();


        //app2.Write(" extra bit.".InGreen());


        //var response = cons.WriteLine("Testing Confirm").Confirm("yarp or narp?", Colour.Cyan, Colour.Magenta);
        //cons.WriteLine($"Response was: {response}");

        var response = cons.StartPrompt("Testing Confirm").AddLine("yarp or narp?".InCyan().OnDarkGrey()).GetConfirmation(Colour.Magenta);
        cons.WriteLine($"Response was: {response}");

        //var response2 = cons.StartPrompt("Single line confirm").Confirm(Colour.Cyan);
        //cons.WriteLine($"Response2 was: {response2}");


        //var response2 = cons.Select<TestEnum>("Select a thingy", TestEnum.That, ConsoleColor.DarkCyan, ConsoleColor.DarkYellow, ConsoleColor.DarkGreen);
        //cons.WriteLine($"Response was: {response2}");

        var enum1 = cons.StartPrompt("Select a ", "thingy".InDarkYellow()).GetEnumSelection<TestEnum>();

        //var enum2 = cons.StartPrompt("Select a ", "thingy".InDarkYellow()).AddLine("with multiple lines".InGreen()).GetEnumSelection<TestEnum>(TestEnum.That, Colour.DarkBlue, Colour.DarkCyan);


        //var first = cons.StartPrompt("type sommat:").GetInput();
        //cons.WriteLine("first: ", first.InYellow());

        //var second = cons.StartPrompt("type sommat".InGreen(), " ya bastard")
        //    .AddLine("with added".InYellow(), " lineage:").GetInput();
        //cons.WriteLine("second: ", second.InYellow());

        //app1.Write(" more".InRed(), " extra".InBlue(), " stuff".InMagenta());

        var third = cons.StartPrompt("Test Multiline Input. type \"", "narf".InGreen(), "\"")
            .AddLine("with added".InYellow(), " lineage").GetFreeText(x => x == "narf");

        var fourth = cons.StartPrompt("Test Single line Input. type \"", "narf".InGreen(), "\" again")
            .GetFreeText(x => x == "narf");

        //app3.Write("nearly done.");

        //app1.Write(" FINISHED");
    }

    public enum TestEnum
    {
        This,
        That,
        TheOther
    }
}


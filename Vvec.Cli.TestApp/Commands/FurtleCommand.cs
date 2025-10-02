using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text;
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

    public enum FurtleMode
    {
        Stuff = 1,
        Status = 2,
        //WritePerf = 3,
    }

    public static string Name => "furtle";

    public static string Description => "Hacky method for dev testing. To aid testing the help output, this command has a rather long description, so the arg parser console can try to do some nice formatting when a line wraps.";

    [Opt('m', "mode", "What type of furtle to do...")]
    public FurtleMode Mode { get; init; }

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
        switch (Mode)
        {
            case FurtleMode.Stuff:
                await FurtleStuff();
                break;

            case FurtleMode.Status:
                await FurtleStatus();
                break;

            //case FurtleMode.WritePerf:
            //    await FurtleWritePerf();
            //    break;

            default:
                cons.WriteLine(FG.DarkRed, "NO FURTLE MODE");
                break;
        }
    }
    //private Task FurtleWritePerf()
    //{

    //    int times = 100;
    //    TimeConsVersion(times);
    //    TimeConsVersion(times);
    //    TimeConsVersion(times);


    //    TimeAnsiCodeVersion(times);
    //    TimeAnsiCodeVersion(times);
    //    TimeAnsiCodeVersion(times);

    //    TimeAnsiCodeVersionWithLocking(times);
    //    TimeAnsiCodeVersionWithLocking(times);
    //    TimeAnsiCodeVersionWithLocking(times);

    //    return Task.CompletedTask;
    //}

    //private void TimeConsVersion(int times)
    //{
    //    var stoppy = new Stopwatch();
    //    stoppy.Start();
    //    for (int i = 0; i < times; i++)
    //        cons.Write("a".InRed(), "b".InDarkRed(), "c".InYellow(), "d".InDarkYellow(), "e".InGrey());
    //    stoppy.Stop();
    //    cons.WriteLine();
    //    cons.WriteLine($"-- {times} runs with Cons took ", stoppy.ElapsedMilliseconds.InDarkYellow(), "ms.");
    //}

    private static object locker = new object();
    //private void TimeAnsiCodeVersion(int times)
    //{
    //    var stoppy = new Stopwatch();
    //    stoppy.Start();
    //    for (int i = 0; i < times; i++)
    //    {
    //            var sb = new StringBuilder();
    //            sb.Append(In.Red) //.ToString())
    //                .Append("a")
    //                .Append(In.DarkRed)//.ToString())
    //                .Append("b")
    //                .Append(In.Yellow)//.ToString())
    //                .Append("c")
    //                .Append(In.DarkYellow)//.ToString())
    //                .Append("d")
    //                .Append(In.Default)//.ToString())
    //                .Append("e");
    //            Console.Write(sb.ToString());
    //    }
    //    stoppy.Stop();
    //    cons.WriteLine();
    //    cons.WriteLine($"-- {times} runs with ANSI codes took ", stoppy.ElapsedMilliseconds.InDarkYellow(), "ms.");
    //}

    //private void TimeAnsiCodeVersionWithLocking(int times)
    //{
    //    var stoppy = new Stopwatch();
    //    stoppy.Start();
    //    for (int i = 0; i < times; i++)
    //    {
    //        lock (locker)
    //        {
    //            var sb = new StringBuilder();
    //            sb.Append(In.Red) //.ToString())
    //                .Append("a")
    //                .Append(In.DarkRed)//.ToString())
    //                .Append("b")
    //                .Append(In.Yellow)//.ToString())
    //                .Append("c")
    //                .Append(In.DarkYellow)//.ToString())
    //                .Append("d")
    //                .Append(In.Default)//.ToString())
    //                .Append("e");
    //            Console.Write(sb.ToString());
    //        }
    //    }
    //    stoppy.Stop();
    //    cons.WriteLine();
    //    cons.WriteLine($"-- {times} runs with ANSI codes & locking took ", stoppy.ElapsedMilliseconds.InDarkYellow(), "ms.");
    //}

    //public class In
    //{
    //    public static readonly AnsiColour Default = new(0);
    //    public static readonly AnsiColour Red = new(91);
    //    public static readonly AnsiColour DarkRed = new(31);
    //    public static readonly AnsiColour Yellow = new(93);
    //    public static readonly AnsiColour DarkYellow = new(33);
    //}

    //public class AnsiColour(int code)
    //{
    //    private static readonly string Escape = new string([(char)27, '[']);
    //    private readonly string sequence = Escape + code.ToString() + "m";

    //    public override string ToString()
    //    {
    //        return sequence;
    //    }
    //}

    public async Task FurtleStatus()
    {
        //var append = cons.StartAppendable("Appendable.").StartStatus("update1");
        //await Task.Delay(1000);
        //append.Update("update number 2".InBlue());
        //await Task.Delay(1000);
        //append.Update("more");
        //await Task.Delay(1000);
        //append.Finish().Write(". That's done now.");

        var append1 = cons.StartAppendable("With spinner").StartStatus(" here").WithSpinner();
        var append2 = cons.StartAppendable("With ellipsis").StartStatus(" here").WithEllipsis();
        await Task.Delay(2000);
        append1.Update(" something happened").WithSpinner();
        append2.Update(" something happened").WithEllipsis();
        await Task.Delay(2300);
        append1.Finish();
        append2.Finish();

        //append = cons.StartAppendable("Appendable.").StartStatus("here's a longer update to write");
        //await Task.Delay(1000);
        //append.Finish("No more left").Write("!".InMagenta());

    }


    public async Task FurtleStuff()
    {
        cons.WriteLine()
            .WriteLine(FG.DarkYellow, "Furtling with the CLI. Selected mode: ", FG.Blue, Mode);

        //var dom = System.Threading.Thread.GetDomain();
        //cons
        //    .WriteLine("Environment.ProcessPath:".PadRight(35), Environment.ProcessPath.InYellow())
        //    .WriteLine("Directory.GetCurrentDirectory:".PadRight(35), Directory.GetCurrentDirectory().InYellow())
        //    .WriteLine("AppDomain.BaseDirectory:".PadRight(35), dom.BaseDirectory.InYellow())
        //    .WriteLine("AppDomain.DynamicDirectory:".PadRight(35), dom.DynamicDirectory.InYellow())
        //    ;

        //cons.WriteLine(Path.GetFullPath(Environment.ProcessPath)).WriteLine(Path.GetDirectoryName(Environment.ProcessPath));

        var bob = cons.StartPrompt("Here's a bunch of random text. tsra tsrnei etsran eitsr itsra neit teitsr. Here's another bunch of random text. tsra tsrnei etsran eitsr itsra neit teitsr:")
            .AddLine("This is a prompt - type something or get a default value")
            .GetFreeTextOrDefault("<default value>");
        cons.WriteLine("Got:", FG.Blue, bob);

        cons
            .WriteLine()
            .WriteLine(FG.DarkRed, BG.DarkYellow, "I should split out several, better named commands from the unreachable code below to check different aspects of the UI stuff...");

        //return;
        //var bufH = System.Console.BufferHeight;
        //var bufW = System.Console.BufferWidth;

        //var winH = Console.WindowHeight;
        //var winW = Console.WindowWidth;
        //var winT = Console.WindowTop;
        //var winL = Console.WindowLeft;

        //var largeH = Console.LargestWindowHeight;
        //var largeW = Console.LargestWindowWidth;

        //var (curL, curT) = Console.GetCursorPosition();

        //cons.WriteLine("Buf:", bufW, "x", bufH, ", Win:", winW, "x", winH, ", Pos:", winL, ":", winT, ", Largest:", largeH, "x", largeW, ", Cur:", curL, "x", curT);


        //return;

        cons.WriteLine(FG.DarkRed, "Hmmm, it looks like the async bits below aren't working now - I'll need to look into that...");

        var app1 = cons.StartAppendable("Appendable1:").StartSpinner();
        var app2 = cons.StartAppendable("Second appendable").StartSpinner();
        await Task.Delay(2000).ConfigureAwait(false);
        cons.WriteLine("1 second");
        await Task.Delay(1000);
        cons.WriteLine("2 seconds");
        app1.Write(" and den").StartSpinner();
        await Task.Delay(1000);
        cons.WriteLine("3 seconds");
        await Task.Delay(1000);
        cons.WriteLine("4 seconds");
        var defaulty = cons.StartPrompt("Text input with a default value (which isn't shown automagically, you need to add that...)").GetFreeTextOrDefault("your mum");
        cons.WriteLine("Got: ", FG.DarkYellow, defaulty);
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

        var response = cons.StartPrompt("Testing Confirm").AddLine(FG.Cyan, BG.DarkGrey, "yarp or narp?").GetConfirmation(FG.Magenta);// Colour.Magenta);
        //var response = cons.StartPrompt("Testing Confirm").AddLine("yarp or narp?".InCyan().OnDarkGrey()).GetConfirmation(Colour.Magenta);
        cons.WriteLine($"Response was: {response}");

        //var response2 = cons.StartPrompt("Single line confirm").Confirm(Colour.Cyan);
        //cons.WriteLine($"Response2 was: {response2}");


        //var response2 = cons.Select<TestEnum>("Select a thingy", TestEnum.That, ConsoleColor.DarkCyan, ConsoleColor.DarkYellow, ConsoleColor.DarkGreen);
        //cons.WriteLine($"Response was: {response2}");

        var enum1 = cons.StartPrompt("Select a ", FG.DarkYellow, "thingy").GetEnumSelection<TestEnum>(defaultValue: TestEnum.That, FG.Yellow, FG.DarkYellow);

        //var enum2 = cons.StartPrompt("Select a ", "thingy".InDarkYellow()).AddLine("with multiple lines".InGreen()).GetEnumSelection<TestEnum>(TestEnum.That, Colour.DarkBlue, Colour.DarkCyan);


        //var first = cons.StartPrompt("type sommat:").GetInput();
        //cons.WriteLine("first: ", first.InYellow());

        //var second = cons.StartPrompt("type sommat".InGreen(), " ya bastard")
        //    .AddLine("with added".InYellow(), " lineage:").GetInput();
        //cons.WriteLine("second: ", second.InYellow());

        //app1.Write(" more".InRed(), " extra".InBlue(), " stuff".InMagenta());

        var third = cons.StartPrompt("Test Multiline Input. type \"", Style.As(FG.Green, "narf"), "\"")
            .AddLine(Style.As(FG.Yellow, "with added"), " lineage").GetFreeText(x => x == "narf");

        var fourth = cons.StartPrompt("Test Single line Input. type \"", Style.As(FG.Green, "narf"), "\" again")
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


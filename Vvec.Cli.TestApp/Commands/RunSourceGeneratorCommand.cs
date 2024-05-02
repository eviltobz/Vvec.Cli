//using System.CommandLine;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;
using Vvec.Sg;

public class RunSourceGeneratorCommand : ISubCommand
{
    private readonly IConsole cons;

    public RunSourceGeneratorCommand(IConsole cons)
    {
        this.cons = cons;
    }

    public static string Name => "sg";

    public static string Description => "Run simple source generator";

    [EnumExtensions]
    public enum SgEnum
    {
        This,
        That,
        TheOther
    }

    public void Execute()
    {
        var sourceGenned = new Sg();
        sourceGenned.SG();

        var a = SgEnum.This;
        var b = SgEnum.That;
        var c = SgEnum.TheOther;

        cons.WriteLine(a.ToStringFast());
        cons.WriteLine(b.ToStringFast());
        cons.WriteLine(c.ToStringFast());

    }
}

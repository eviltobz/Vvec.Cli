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
        SgThis,
        SgThat,
        SgTheOther
    }

    public void Execute()
    {
        //var sourceGenned = new Sg();
        //sourceGenned.SG();

        var a = SgEnum.SgThis;
        var b = SgEnum.SgThat;
        var c = SgEnum.SgTheOther;

        cons.WriteLine(a.ToStringFast());
        cons.WriteLine(b.ToStringFast());
        cons.WriteLine(c.ToStringFast());

    }
}

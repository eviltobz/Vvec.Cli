namespace Vvec.Cli.SourceGenerator;

public readonly struct ClassToRegister
{
    public readonly string Name;
    public readonly CliModifier[] Arguments;
    public readonly string debugString;

    public ClassToRegister(string name, CliModifier[] arguments, string debugString)
    {
        this.debugString = debugString;
        Name = name;
        Arguments = arguments;
    }
}

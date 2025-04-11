namespace Vvec.Cli.SourceGenerator;

public readonly struct ClassToRegister
{
    public readonly string Name;
    public readonly CliModifier[] Arguments;
    public readonly string debugString;
    public readonly bool IsAsync;

    public ClassToRegister(string name, CliModifier[] arguments, string debugString, bool isAsync)
    {
        this.debugString = debugString;
        Name = name;
        Arguments = arguments;
        IsAsync = isAsync;
    }
}

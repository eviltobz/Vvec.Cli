namespace Vvec.Cli.SourceGenerator;

public readonly struct Argument : CliModifier
{
    public readonly string Name { get; }
    private readonly string Type { get; }
    private readonly string CliName { get; }
    private readonly string CliDescription { get; }

    public Argument(string name, string type, string cliName, string cliDescription)
    {
        Name = name;
        Type = type;
        CliName = cliName;
        CliDescription = cliDescription;
    }

    public string GenerateRegistration() => @"
                var " + Name + " = new Argument<" + Type + ">("
            + @"""" + CliName + @""", """ + CliDescription + @""");
                " + Name + @".Arity = ArgumentArity.ZeroOrOne;
                command.AddArgument(" + Name + ");";
    // Note, I initially did .Arity with some expectation of having a required flag, so this might be odd
    // with some arg setups until that is more fully fleshed out...
}

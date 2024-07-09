namespace Vvec.Cli.SourceGenerator;

public readonly struct Option : CliModifier
{
    public readonly string Name { get; }
    private readonly string Type { get; }
    private readonly char CliFlag { get; }
    private readonly string CliName { get; }
    private string CliDescription { get; }

    public Option(string name, string type, char cliFlag, string cliName, string cliDescription)
    {
        Name = name;
        Type = type;
        CliFlag = cliFlag;
        CliName = cliName;
        CliDescription = cliDescription;
    }

    public string GenerateRegistration() => @"
                var " + Name + " = new Option<" + Type + ">("
            + @"""-" + CliFlag + @""", """ + CliDescription + @""");
                " + Name + @".AddAlias(""--" + CliName + @""");
                command.AddOption(" + Name + ");";
}

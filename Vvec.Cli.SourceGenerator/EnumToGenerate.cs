using System.Text;

namespace Vvec.Cli.SourceGenerator
{
    public readonly struct EnumToGenerate
    {
        public readonly string Name;
        public readonly List<string> Values;

        public EnumToGenerate(string name, List<string> values)
        {
            Name = name;
            Values = values;
        }
    }

    //public enum ArgKind
    //{
    //    Argument,
    //    Option
    //}

    public interface CliModifier
    {
        string Name { get; }
        string GenerateRegistration();
    }

    public readonly struct Argument : CliModifier
    {
        public readonly string Name { get; }
        private readonly string Type { get; }
        private readonly string CliName { get; }
        private string CliDescription { get; }

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
                command.AddArgument(" + Name + ");";
    }

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

    public readonly struct ClassToRegister
    {
        public readonly string Name;
        public readonly CliModifier[] Arguments;
        public readonly string debuggery;

        public ClassToRegister(string name, CliModifier[] arguments, string debuggery)
        {
            this.debuggery = debuggery;
            Name = name;
            Arguments = arguments;
        }
    }
}

namespace Vvec.Cli.SourceGenerator;

public interface CliModifier
{
    string Name { get; }
    string GenerateRegistration();
}

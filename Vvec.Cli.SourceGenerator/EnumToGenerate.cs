// This was taken from a source generator tutorial and isn't really part of this library - it's just left in for further reference
namespace Vvec.Cli.SourceGenerator;

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

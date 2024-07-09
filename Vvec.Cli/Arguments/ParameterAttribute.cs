namespace Vvec.Cli.Arguments;

[AttributeUsage(AttributeTargets.Property)]
public class ArgAttribute : Attribute
{
    // These fields are accessed by the Source Generator, so we don't need to do anything with the parameters here.
    public ArgAttribute(string name, string description)
    { }
}

[AttributeUsage(AttributeTargets.Property)]
public class OptAttribute : Attribute
{
    // These fields are accessed by the Source Generator, so we don't need to do anything with the parameters here.
    public OptAttribute(char flag, string name, string description)
    { }
}

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq.Expressions;

namespace Vvec.Cli.Arguments
{
    /*
    maybe revisit the idea of parsing methods for args at some point. the attribute could just be the name of a static 
    function on the argument type. yeah, it's not nice & typesafe, but it is better than nowt. As it handles ints & bools
    (and presumably some other simple types) out of the box, it'd be a bit weird for everything else to need to be a string.
    I could have my own, simpler interface than what is needed for System.Commandline, and wrap it up when registering...
     */ 
    public interface ISubCommand : ISubCommandBase
    {
        public void Execute();
    }

    public interface ISubCommand<TArgs> : ISubCommandBase, ISubCommandWithArguments//<TArgs>
    {
        void Execute(TArgs args);
        //public static abstract IEnumerable<Symbol> Modifiers { get; }
    }

    public interface ISubCommandAsync : ISubCommandBase
    {
        public Task Execute();
    }

    public interface ISubCommandAsync<TArgs> : ISubCommandBase, ISubCommandWithArguments//<TArgs>
    {
        public Task Execute(TArgs args);
        //public static abstract IEnumerable<Symbol> Modifiers { get; }
    }
    
    public record ArgParser<T> (Expression<T> Property, Func<ArgumentResult, T> parser);

    public interface ISubCommandWithArguments//<TArgs>
    {
        //public static abstract IEnumerable<Symbol> Modifiers { get; }

        //public TArgs BuildArguments();

    }

    public interface ISubCommandBase
    {
        public static abstract string Name { get; }

        public static abstract string Description { get; }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    //public class CmdAttribute : Attribute
    //{
    //    public string Name { get; init; }

    //    public string Description { get; init; }

    //    public CmdAttribute(string name, string description)
    //    {
    //        Name = name;
    //        Description = description;
    //    }
    //}

    [AttributeUsage(AttributeTargets.Property)]
    public class ArgAttribute : Attribute
    {
        public string Name { get; init; }

        public string Description { get; init; }

        public ArgAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ArgxAttribute<T> : ArgAttribute
    {
        //public ParseArgument<T> Parser { get; init; }

        public ArgxAttribute(string name, string description, Func<ArgumentResult, T> parser) : base(name, description)
        //public ArgxAttribute(string name, string description, ParseArgument<T> parser) : base(name, description)
        {
            //Name = name;
            //Description = description;
            //Parser = parser;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptAttribute : Attribute
    {
        public string Name { get; init; }

        public string Description { get; init; }

        public OptAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

namespace Vvec.Cli.Arguments;

public interface ISubCommand : ISubCommandBase
{
    public void Execute();
}

public interface ISubCommand<TArgs> : ISubCommandBase
{
    void Execute(TArgs args);
}

public interface ISubCommandAsync : ISubCommandBase
{
    public Task Execute();
}

public interface ISubCommandAsync<TArgs> : ISubCommandBase
{
    public Task Execute(TArgs args);
}

public interface ISubCommandBase
{
    public static abstract string Name { get; }

    public static abstract string Description { get; }
}

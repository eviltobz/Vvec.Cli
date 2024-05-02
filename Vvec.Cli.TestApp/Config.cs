using Vvec.Cli.Config;

public class Config
{
    public string? StringValue { get; init; }

    public int IntValue { get; init; }

    public FolderPath? FolderPath { get; init; }

    public FilePath? FilePath { get; init; }

    public Url? Url { get; init; }
}


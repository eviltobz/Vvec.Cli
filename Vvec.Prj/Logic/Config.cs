using Vvec.Cli.Config;

namespace Vvec.Prj.Logic;

public class Config
{
    public static Config CreateDefault() =>
        new Config()
        {
            Shortcuts = new()
            {
                { "eviltobz", new FolderPath(){ Value = @"c:\git\eviltobz" } },
                { "axis", new FolderPath(){ Value = @"c:\git\Axis" } },
                { "bu", new FolderPath(){ Value = @"c:\git\dyn\dyn-backend\utilities" } },
            }
        };

    public FolderPath? ProjectRoot { get; init; }

    public Dictionary<string, FolderPath> Shortcuts { get; init; }
}

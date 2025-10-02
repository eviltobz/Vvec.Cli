using System.Text.Json;
using Vvec.Cli.UI;

namespace Vvec.Cli.Config;

public class ConfigStore<TConfig> where TConfig : new()
{
    private const string ConfigFile = @"appconfig.json";
    private readonly TConfig? defaultConfig;
    private readonly IConsole cons;

    public bool FileLoadError { get; private set; } = false;

    public string Path { get; private init; }

    public ConfigStore(TConfig? defaultConfig, IConsole cons)
    {
        this.cons = cons;
        this.defaultConfig = defaultConfig;
        var dir = System.IO.Path.GetDirectoryName(Environment.ProcessPath)!;
        Path = System.IO.Path.Combine(dir, ConfigStore<TConfig>.ConfigFile);
    }

    public TConfig? Read()
    {
        if (File.Exists(Path))
            return LoadConfig();
        else
            return CreateConfig();
    }

    public void Write(TConfig newConfig)
    {
        var opts = new JsonSerializerOptions() { WriteIndented = true };
        File.WriteAllText(Path, JsonSerializer.Serialize(newConfig, opts));
    }

    private TConfig? LoadConfig()
    {
        try
        {
            var config = File.ReadAllText(Path);
            return JsonSerializer.Deserialize<TConfig>(config)!;
        }
        catch(Exception ex) // We could check for any exception here, or just presume that if it throws it's a deserialisation issue...
        {
            FileLoadError = true;
            cons.WriteLine(FG.Red, "Error loading config.", FG.Magenta, ex.Message);
            //cons.WriteLine("Error loading config.".InRed(), ex.Message.InMagenta());
            return default(TConfig);
        }
    }

    private TConfig CreateConfig()
    {
        if (defaultConfig is null)
        {
            var emptyConfig = new TConfig();
            Write(emptyConfig);
            return emptyConfig;
        }

        Write(defaultConfig);
        return defaultConfig;
    }
}


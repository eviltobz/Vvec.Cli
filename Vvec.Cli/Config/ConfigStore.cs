using System.Text.Json;

namespace Vvec.Cli.Config;




public class ConfigStore<TConfig> where TConfig : new()
{
    private const string ConfigFile = @"appconfig.json";
    private readonly string path;
    private readonly TConfig? defaultConfig;

    public ConfigStore(TConfig? defaultConfig)
    {
        this.defaultConfig = defaultConfig;
        var dir = Path.GetDirectoryName(Environment.ProcessPath)!;
        path = Path.Combine(dir, ConfigFile);
    }

    public TConfig Read()
    {
        if (File.Exists(path))
            return LoadConfig();
        else
            return CreateConfig();
    }

    public void Write(TConfig newConfig)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(newConfig));
    }

    private TConfig LoadConfig()
    {
        var config = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TConfig>(config)!;
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


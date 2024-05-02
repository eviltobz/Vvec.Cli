﻿using Vvec.Cli.Arguments;
using Vvec.Cli.UI;
using IConsole = Vvec.Cli.UI.IConsole;

namespace Vvec.Cli.Config;
public class ConfigCommand<TConfig> : ISubCommand where TConfig : class, new()
{
    private readonly IConsole cons;
    private readonly TConfig config;
    private readonly ConfigStore<TConfig> configStore;

    [Arg("Key", "Name (or camel-cased initials) of the config property")]
    public string Key { get; init; }

    [Arg("Value", "The value to store")]
    public string Value { get; init; }

    public ConfigCommand(IConsole cons, ConfigStore<TConfig> configStore)
    {
        this.configStore = configStore;
        this.config = configStore.Read();
        this.cons = cons;
        Key = string.Empty;
        Value = string.Empty;
    }

    public static string Name => "config";

    public static string Description => "View or modify configuration. Run without arguments to view or supply the key & value to set.";

    public void Execute()
    {
        if (string.IsNullOrEmpty(Key))
            DisplayConfig();
        else
            SetConfig();
    }

    // Config collections:
    // * In all cases, content could be string, other base type, or validatable - such as folders for prj
    // Dictionary<string name, TValue content>
    // * only allows distinct keys
    // List<KeyValuePair<string name, TValue content>
    // * allows multiple entries for the same key
    //   * not sure when I'd need it, but it sounds feasible if you might want a 0-many type config
    //
    // cli config add collectionName key value
    // cli config rm collectionName key
    //
    // Hashset<TValue content>
    // * Similar to above, allows multiple entries, we only have a single batch, rather than multiple ones
    //   * e.g. could be a bunch of paths to check for something, you don't care where it's coming from...
    // * hashy type collection as we don't want multiple
    // * need fuzzy matching similar to field names on the main config so you could remove it without needing the full thing
    private void SetConfig()
    {
        var updater = new ConfigUpdater<TConfig>(config, Key, Value);

        if (updater.MatchingFieldCount == 0)
        {
            cons.WriteLine("Field '".InRed(), Key.InYellow(), "' not found".InRed());
            return;
        }
        if (updater.MatchingFieldCount > 1)
        {
            cons.WriteLine("Multiple matches for field '".InRed(), Key.InYellow(), "' found".InRed());
            foreach (var (candidateName, candidateType) in updater.MatchingFields)
                cons.WriteLine("  ", candidateName.InYellow(), $" ({candidateType})");
            return;
        }

        var (name, type) = updater.MatchingFields.Single();
        if (updater.FailedValidation)
        {
            cons.WriteLine("Invalid content for field '".InRed(), name.InYellow(), "'. Expected a valid ".InRed(), type.InYellow(), " but got '".InRed(), Value.InYellow(), "'".InRed());
            return;
        }

        cons.WriteLine("Old Config:");
        DisplayConfig(name);

        updater.UpdateConfig();
        configStore.Write(config);

        cons.WriteLine("New Config:");
        DisplayConfig(name);
    }

    private static readonly Type OpenType = typeof(IValidateConfig<>);

    private void DisplayConfig(string? targetProperty = null)
    {
        var props = config.GetType().GetProperties();

        var maxLen = props.Select(p => p.Name.Length).Max();

        foreach (var prop in props)
        {
            object itemValue = null;
            if (prop.PropertyType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == OpenType))
            {
                var value = prop.PropertyType.GetProperty(nameof(IValidateConfig<bool>.Value))!;
                var validatable = prop.GetValue(config);
                if (validatable is not null)
                    itemValue = value.GetValue(validatable);
            }
            else
                itemValue = prop.GetValue(config);


            if (prop.Name.ToLower() == targetProperty?.ToLower())
                cons.WriteLine(">>", prop.Name.PadRight(maxLen).InDarkYellow(), ": ", itemValue.InDarkGreen());
            else
                cons.WriteLine("  ", prop.Name.PadRight(maxLen).InYellow(), ": ", itemValue.InGreen());
        }
    }

}
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;
using IConsole = Vvec.Cli.UI.IConsole;

namespace Vvec.Cli.Config;
public class ConfigCommand<TConfig> : ISubCommand where TConfig : class, new()
{
    private readonly IConsole cons;
    private readonly TConfig config;
    private readonly ConfigStore<TConfig> configStore;

    // NOTE: If changing args here you need to update Initialiser.Execute to know about them too.
    [Arg("Key", "Name (or camel-cased initials) of the config property")]
    public string Key { get; init; }

    [Arg("Value", "The value to store")]
    public string Value { get; init; }

    [Opt('e', "edit", "Open config file in Vim (Ignores any provided Key/Value pair)")]
    public bool Edit { get; init; }

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
        if (configStore.FileLoadError)
        {
            cons.StartPrompt("Opening config for manual editing. ").PressAnyKey();
        }

        if (Edit || configStore.FileLoadError)
            OpenInVim();
        else if (string.IsNullOrEmpty(Key))
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
            //cons.WriteLine("Field '".InRed(), Key.InYellow(), "' not found".InRed());
            cons.WriteLine(FG.Red, "Field '", FG.Yellow, Key, FG.Red, "' not found");
            return;
        }
        if (updater.MatchingFieldCount > 1)
        {
            cons.WriteLine(FG.Red, "Multiple matches for field '", FG.Yellow, Key, FG.Red, "' found");
            //cons.WriteLine("Multiple matches for field '".InRed(), Key.InYellow(), "' found".InRed());
            foreach (var (candidateName, candidateType) in updater.MatchingFields)
                cons.WriteLine("  ", FG.Yellow, candidateName, FG.Default, $" ({candidateType})");
                //cons.WriteLine("  ", candidateName.InYellow(), $" ({candidateType})");
            return;
        }

        var (name, type) = updater.MatchingFields.Single();
        if (updater.FailedValidation)
        {
            //cons.WriteLine("Invalid content for field '".InRed(), name.InYellow(), "'. Expected a valid ".InRed(), type.InYellow(), " but got '".InRed(), Value.InYellow(), "'".InRed());
            cons.WriteLine(FG.Red, "Invalid content for field '", FG.Yellow, name, FG.Red, "'. Expected a valid ", FG.Yellow, type, FG.Red, " but got '", FG.Yellow, Value, FG.Red, "'");
            return;
        }

        cons.WriteLine("Old Config:");
        DisplayConfig(name);

        updater.UpdateConfig();
        configStore.Write(config);

        cons.WriteLine("New Config:");
        DisplayConfig(name);
    }

    private static readonly Type ValidatableType = typeof(IValidateConfig<>);
    private static readonly Type KeyValueTypeGen = typeof(KeyValuePair<,>);
    private static readonly Type CollectionTypeGen = typeof(ICollection<>);
    private static readonly Type CollectionType = typeof(ICollection);

    private void DisplayConfig(string? targetProperty = null)
    {
        var props = config.GetType().GetProperties();

        var maxLen = props.Select(p => p.Name.Length).Max();

        foreach (var prop in props)
        {
            object itemValue = null;
            var interfaces = prop.PropertyType.GetInterfaces();
            if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == CollectionTypeGen))
            {
                if (!interfaces.Any(i => i == CollectionType))
                {
                    cons.WriteLine("Unknown collection type for " + prop.ToString());
                    continue;
                }

                var collection = (ICollection)prop.GetValue(config);
                var formatted = new List<KeyValuePair<string, string>>();
                foreach (var unknown in collection)
                {
                    var unknownType = unknown.GetType();
                    var unknownGeneric = unknownType.GetGenericTypeDefinition();
                    if (unknownGeneric == KeyValueTypeGen && unknownType.GenericTypeArguments[0] == typeof(string))
                    {
                        var key = unknownType.GetProperty("Key").GetValue(unknown) as string;
                        var valueProp = unknownType.GetProperty("Value");

                        var value = GetConfigValue(valueProp, valueProp.PropertyType.GetInterfaces(), unknown);

                        formatted.Add(new(key, value.ToString()));
                    }
                }

                var maxKeyLen = formatted.Select(x => x.Key.Length).Max();
                //cons.WriteLine("  ", prop.Name.InDarkCyan(), ":");
                cons.WriteLine("  ", FG.DarkCyan, prop.Name, FG.Default, ":");

                foreach (var item in formatted.OrderBy(kvp => kvp.Key))
                {
                    //cons.WriteLine("    ", item.Key.PadRight(maxKeyLen).InYellow(), ": ", item.Value.ToString().InGreen());
                    cons.WriteLine("    ", FG.Yellow, item.Key.PadRight(maxKeyLen), FG.Default, ": ", FG.Green, item.Value.ToString());
                }
            }
            else
            {
                itemValue = GetConfigValue(prop, interfaces, config);

                if (prop.Name.ToLower() == targetProperty?.ToLower())
                    cons.WriteLine(">>", FG.DarkYellow, prop.Name.PadRight(maxLen), FG.Default, ": ", FG.DarkGreen, itemValue);
                //cons.WriteLine(">>", prop.Name.PadRight(maxLen).InDarkYellow(), ": ", itemValue.InDarkGreen());
                else
                    cons.WriteLine("  ", FG.Yellow, prop.Name.PadRight(maxLen), FG.Default, ": ", FG.Green, itemValue);
                    //cons.WriteLine("  ", prop.Name.PadRight(maxLen).InYellow(), ": ", itemValue.InGreen());
            }
        }
    }

    // Passing in the interfaces when we already have the propertyInfo feels a bit redundant...
    private object GetConfigValue(PropertyInfo prop, Type[] interfaces, object source)
    {
        var validatableType = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == ValidatableType);
        if (validatableType is not null)
        {
            var value = prop.PropertyType.GetProperty(nameof(IValidateConfig<bool>.Value));
            var validatableProperty = prop.GetValue(source);
            if (validatableProperty is not null)
                return value.GetValue(validatableProperty);

            return "BORK!";
        }
        else
            return prop.GetValue(source);
    }

    private void OpenInVim()
    {
        var vim = Process.Start("vim.exe", configStore.Path);
        vim.WaitForExit();
    }
}

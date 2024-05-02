using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vvec.Cli.Config;

public class ConfigUpdater<TConfig> where TConfig : class, new()
{
    private static readonly Type OpenType = typeof(IValidateConfig<>);

    private readonly TConfig _config;
    private readonly string _content;
    private object? _validatedValue;
    private readonly PropertyInfo[] _allProps;
    private readonly PropertyInfo[] _matchingProps;
    private readonly PropertyInfo? prop;


    public ConfigUpdater(TConfig config, string key, string content)
    {
        _content = content;
        _config = config;
        _allProps = config.GetType().GetProperties();
        _matchingProps = GetProperty(key);

        prop = _matchingProps.FirstOrDefault();
    }

    public int MatchingFieldCount => _matchingProps.Length;

    public IEnumerable<(string Name, string Type)> MatchingFields
    {
        get => _matchingProps.Select<PropertyInfo, (string, string)>(p => new(p.Name, p.PropertyType.Name));
    }

    public bool FailedValidation
    {
        get
        {
            if (prop.PropertyType == typeof(string))
            {
                _validatedValue = _content;
                return false;
            }
            if (prop.PropertyType == typeof(int))
            {
                var success = int.TryParse(_content, out var validValue);
                if (!success)
                    return true;
                _validatedValue = validValue;
                return false;
            }
            if (prop.PropertyType == typeof(bool))
            {
                var success = bool.TryParse(_content, out var validValue);
                if (!success)
                    return true;

                _validatedValue = validValue;
                return false;
            }
            else if (prop.PropertyType.GetInterfaces().Any(i => i.GetGenericTypeDefinition() == OpenType))
            {
                var method = prop.PropertyType.GetMethod(nameof(IValidateConfig<bool>.TryCreate))!;
                var item = method.Invoke(null, new[] { _content });
                if (item is null)
                    return true;

                _validatedValue = item;
                return false;
            }
            throw new Exception($"Unknown type:{prop.PropertyType.Name}, can't validate");
        }
    }

    public void UpdateConfig()
    {
        prop.SetValue(_config, _validatedValue);
    }

    private PropertyInfo[] GetProperty(string fieldName)
    {
        var normalisedName = fieldName.ToUpper();
        var byFullName = _allProps.Where(p => p.Name.ToUpper() == normalisedName);
        if (byFullName.Any())
            return byFullName.ToArray();

        var byInitials = _allProps.Where(p =>
        {
            var initials = new string(p.Name.Where(c => char.IsUpper(c)).ToArray());
            return normalisedName == initials;
        });

        return byInitials.ToArray();
    }
}
public interface IValidateConfig<TItem>
{
    public static abstract IValidateConfig<TItem>? TryCreate(string value);

    public TItem Value { get; }
}

// Move these to the library & do ctor validations on em.
// Want validations to fail hard when setting the value from the command handler
// but not when loading it up from disk, or creating a default config
// Maybe add internal IsValid flag so the cli apps don't see it, then the command handler
// can check when updating config, and the view config could also check & flag up problems with stored values
// but we'd not auto check all of em at run time - a url may only be needed for one command,
// depending on other flags, so we dont wanna impose time to check that if we're not using it.
// (not saying we should have a way to validate when a command reads that config either, just let it fail)
public class FilePath : IValidateConfig<string>
{
    public string Value { get; init; }

    public static IValidateConfig<string>? TryCreate(string value)
    {
        if (!File.Exists(value))
            return null;

        return new FilePath { Value = value };
    }
}

// can we even override the ctor on a record & bail from it?
public class FolderPath : IValidateConfig<string>
{
    public string Value { get; init; }

    public static IValidateConfig<string>? TryCreate(string value)
    {
        if (!Directory.Exists(value))
            return null;

        return new FolderPath { Value = value };
    }
}

public class Url : IValidateConfig<string>
{
    public string Value { get; init; }

    public static IValidateConfig<string>? TryCreate(string value) => throw new NotImplementedException();
}


// Check out the SecureString type, that might be handy
// Enter a single password to encrypt & decrypt all instances of secrets in the config
// Need to find a suitable, single password en/de-crypt algorithm.
// Only require the password if you're _actively_ using a secret, e.g. saving it, or
// using it in a 'proper' command - let the display config stuff just show some *s
public class Secret
{ }

// How should we do a custom validation?
// * attribute on the field in the config class?
//   * Nice for using the config, maybe not so nice for creating it
// * base generic type with an abstract predicate?
//   * Maybe better for creating the config, but you need to get the inner type to use it



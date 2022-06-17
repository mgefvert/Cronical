using System.Reflection;

namespace Cronical.Integrations;

/// <summary>
/// Base class for manipulating properties of an object through text-based Get and Set methods.
/// </summary>
public abstract class AbstractSettings
{
    public bool Exists(string setting)
    {
        return GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Any(p => p.Name.Equals(setting, StringComparison.InvariantCultureIgnoreCase));
    }

    public bool Set(string setting, string value)
    {
        if (!Exists(setting))
            return false;

        var prop = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Single(p => p.Name.Equals(setting, StringComparison.InvariantCultureIgnoreCase));

        if (prop.PropertyType == typeof(bool))
        {
            if (bool.TryParse(value, out var val))
                prop.SetValue(this, val, null);
            else
                Log.Error($"Value '{value}' is not recognized as a boolean value");
        }
        else if (prop.PropertyType == typeof(int))
        {
            if (int.TryParse(value, out var val))
                prop.SetValue(this, val, null);
            else
                Log.Error($"Value '{value}' is not recognized as an integer value");
        }
        else
            prop.SetValue(this, value, null);

        return true;
    }

    public override string ToString()
    {
        var props = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => (p.GetValue(this, null) ?? "").ToString())
            .ToList();

        return string.Join(",", props);
    }
}
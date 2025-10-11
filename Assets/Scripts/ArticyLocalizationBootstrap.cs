using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Forces Articy's localization system to use English when the game boots.
/// Uses reflection so it remains resilient even if the underlying Articy API changes.
/// </summary>
public static class ArticyLocalizationBootstrap
{
    private const string TargetLanguage = "en";
    private static readonly string[] LanguageAliases = { "en", "eng", "english", "en-us", "en_gb" };
    private const string LogPrefix = "[ArticyLocalizationBootstrap]";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (TrySetLanguageToEnglish())
        {
            Debug.Log($"{LogPrefix} Forced Articy localization to English.");
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} Failed to set Articy localization to English.");
        }
    }

    private static bool TrySetLanguageToEnglish()
    {
        var managerType = FindLocalizationManagerType();
        if (managerType == null)
        {
            Debug.LogWarning($"{LogPrefix} Could not find Articy localization manager type.");
            return false;
        }

        var instance = GetManagerInstance(managerType);

        if (TryAssignLanguageProperties(managerType, instance))
            return true;

        if (TryInvokeLanguageMethods(managerType, instance))
            return true;

        return false;
    }

    private static Type FindLocalizationManagerType()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = null;
            try
            {
                type = assembly.GetType("Articy.Unity.ArticyLocalizationManager")
                       ?? assembly.GetType("Articy.Unity.Localization.ArticyLocalizationManager")
                       ?? assembly.GetTypes().FirstOrDefault(t => t.Name == "ArticyLocalizationManager");
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    Debug.LogWarning($"{LogPrefix} Reflection load warning: {loaderException.Message}");
                }
            }
            catch (Exception)
            {
            }

            if (type != null)
                return type;
        }

        return null;
    }

    private static object GetManagerInstance(Type managerType)
    {
        var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var propertyNames = new[] { "Instance", "Default", "Singleton" };
        foreach (var name in propertyNames)
        {
            var prop = managerType.GetProperty(name, flags);
            if (prop != null)
            {
                try
                {
                    var value = prop.GetValue(null, null);
                    if (value != null)
                        return value;
                }
                catch (Exception)
                {
                }
            }

            var field = managerType.GetField(name, flags);
            if (field != null)
            {
                try
                {
                    var value = field.GetValue(null);
                    if (value != null)
                        return value;
                }
                catch (Exception)
                {
                }
            }
        }

        var methodNames = new[] { "GetInstance", "Get", "Instance" };
        foreach (var name in methodNames)
        {
            var method = managerType.GetMethod(name, flags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                try
                {
                    var value = method.Invoke(null, null);
                    if (value != null)
                        return value;
                }
                catch (Exception)
                {
                }
            }
        }

        var ctor = managerType.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
        {
            try
            {
                return ctor.Invoke(null);
            }
            catch (Exception)
            {
            }
        }

        return null;
    }

    private static bool TryAssignLanguageProperties(Type managerType, object instance)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var propertyNames = new[] { "CurrentLanguage", "Language", "CurrentTextLanguage", "CurrentLocalization", "CurrentCulture" };
        foreach (var name in propertyNames)
        {
            var prop = managerType.GetProperty(name, flags);
            if (prop == null || !prop.CanWrite)
                continue;

            var accessors = prop.GetAccessors(true);
            var target = accessors != null && accessors.Length > 0 && accessors[0].IsStatic ? null : instance;
            if (target == null && !(accessors != null && accessors.Length > 0 && accessors[0].IsStatic))
                continue;

            var value = ConvertLanguageArgument(prop.PropertyType, instance, managerType);
            if (value == null && prop.PropertyType != typeof(string))
                continue;

            try
            {
                prop.SetValue(target, value ?? TargetLanguage, null);
                return true;
            }
            catch (Exception)
            {
            }
        }

        var fieldNames = new[] { "CurrentLanguage", "Language", "CurrentTextLanguage" };
        foreach (var name in fieldNames)
        {
            var field = managerType.GetField(name, flags);
            if (field == null || field.IsInitOnly)
                continue;

            var target = field.IsStatic ? null : instance;
            if (target == null && !field.IsStatic)
                continue;

            var value = ConvertLanguageArgument(field.FieldType, instance, managerType);
            if (value == null && field.FieldType != typeof(string))
                continue;

            try
            {
                field.SetValue(target, value ?? TargetLanguage);
                return true;
            }
            catch (Exception)
            {
            }
        }

        return false;
    }

    private static bool TryInvokeLanguageMethods(Type managerType, object instance)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        foreach (var method in managerType.GetMethods(flags))
        {
            if (!method.Name.Contains("Language", StringComparison.OrdinalIgnoreCase))
                continue;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                continue;

            var parameter = parameters[0];
            var argument = ConvertLanguageArgument(parameter.ParameterType, instance, managerType);
            if (argument == null && parameter.ParameterType != typeof(string))
                continue;

            try
            {
                method.Invoke(method.IsStatic ? null : instance, new object[] { argument ?? TargetLanguage });
                return true;
            }
            catch (Exception)
            {
            }
        }

        return false;
    }

    private static object ConvertLanguageArgument(Type targetType, object managerInstance, Type managerType)
    {
        if (targetType == typeof(string))
            return TargetLanguage;

        if (targetType.IsEnum)
        {
            foreach (var value in Enum.GetValues(targetType))
            {
                var name = Enum.GetName(targetType, value);
                if (IsLanguageMatch(name))
                    return value;
            }
            return null;
        }

        if (targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(short) ||
            targetType == typeof(ushort) || targetType == typeof(long) || targetType == typeof(ulong) ||
            targetType == typeof(byte) || targetType == typeof(sbyte))
        {
            var candidate = FindLanguageCandidate(managerInstance, managerType);
            if (candidate == null)
                return null;

            try
            {
                return Convert.ChangeType(candidate.Index, targetType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
        }

        var objectCandidate = FindLanguageCandidate(managerInstance, managerType);
        if (objectCandidate == null)
            return null;

        if (targetType.IsInstanceOfType(objectCandidate.Value))
            return objectCandidate.Value;

        if (targetType.IsInstanceOfType(objectCandidate.Item))
            return objectCandidate.Item;

        if (!targetType.IsValueType)
        {
            var ctor = targetType.GetConstructor(new[] { typeof(string) });
            if (ctor != null)
            {
                try
                {
                    return ctor.Invoke(new object[] { TargetLanguage });
                }
                catch (Exception)
                {
                }
            }
        }

        return null;
    }

    private static LanguageCandidate FindLanguageCandidate(object managerInstance, Type managerType)
    {
        var languages = GetAvailableLanguages(managerInstance, managerType);
        if (languages == null)
            return null;

        var index = 0;
        foreach (var entry in languages)
        {
            var candidate = ExtractCandidate(entry, index);
            if (candidate != null)
                return candidate;
            index++;
        }

        return null;
    }

    private static IEnumerable GetAvailableLanguages(object instance, Type managerType)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var names = new[] { "AvailableLanguages", "Languages", "SupportedLanguages", "LocalizationLanguages", "AllLanguages" };
        foreach (var name in names)
        {
            var prop = managerType.GetProperty(name, flags);
            if (prop != null)
            {
                try
                {
                    var accessors = prop.GetAccessors(true);
                    var target = accessors != null && accessors.Length > 0 && accessors[0].IsStatic ? null : instance;
                    var value = prop.GetValue(target, null);
                    if (value is IEnumerable enumerable)
                        return enumerable;
                }
                catch (Exception)
                {
                }
            }

            var field = managerType.GetField(name, flags);
            if (field != null)
            {
                try
                {
                    var target = field.IsStatic ? null : instance;
                    var value = field.GetValue(target);
                    if (value is IEnumerable enumerable)
                        return enumerable;
                }
                catch (Exception)
                {
                }
            }
        }

        return null;
    }

    private static LanguageCandidate ExtractCandidate(object entry, int index)
    {
        if (entry == null)
            return null;

        if (entry is IDictionary dictionary)
        {
            foreach (DictionaryEntry pair in dictionary)
            {
                if (TryBuildCandidate(pair.Key, pair.Value, index, out var candidate))
                    return candidate;
            }
            return null;
        }

        if (entry is string str)
        {
            if (IsLanguageMatch(str))
            {
                return new LanguageCandidate
                {
                    Item = str,
                    Value = str,
                    Index = index,
                    Identifier = str,
                };
            }
            return null;
        }

        var type = entry.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var keyProp = type.GetProperty("Key");
            var valueProp = type.GetProperty("Value");
            var keyValue = keyProp?.GetValue(entry);
            var valueValue = valueProp?.GetValue(entry);
            if (TryBuildCandidate(keyValue, valueValue, index, out var candidate))
                return candidate;
        }

        var identifier = GetIdentifier(entry);
        if (IsLanguageMatch(identifier))
        {
            return new LanguageCandidate
            {
                Item = entry,
                Value = entry,
                Index = index,
                Identifier = identifier,
            };
        }

        return null;
    }

    private static bool TryBuildCandidate(object key, object value, int index, out LanguageCandidate candidate)
    {
        candidate = null;
        var keyId = key?.ToString();
        var valueId = value?.ToString();

        if (IsLanguageMatch(keyId))
        {
            candidate = new LanguageCandidate
            {
                Item = value ?? key,
                Value = value ?? key,
                Index = index,
                Identifier = keyId,
            };
            return true;
        }

        if (IsLanguageMatch(valueId))
        {
            candidate = new LanguageCandidate
            {
                Item = value ?? key,
                Value = value ?? key,
                Index = index,
                Identifier = valueId,
            };
            return true;
        }

        return false;
    }

    private static string GetIdentifier(object obj)
    {
        if (obj == null)
            return null;

        var type = obj.GetType();
        var propertyNames = new[] { "Identifier", "Id", "Name", "TechnicalName", "CultureKey", "TwoLetterIsoCode", "Culture", "Code", "LanguageName" };
        foreach (var name in propertyNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null)
                continue;

            try
            {
                var value = prop.GetValue(obj, null);
                if (value != null)
                {
                    var text = value.ToString();
                    if (!string.IsNullOrEmpty(text))
                        return text;
                }
            }
            catch (Exception)
            {
            }
        }

        return obj.ToString();
    }

    private static bool IsLanguageMatch(string candidate)
    {
        if (string.IsNullOrEmpty(candidate))
            return false;

        foreach (var alias in LanguageAliases)
        {
            if (string.Equals(candidate, alias, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var normalized = candidate.Replace("-", string.Empty).Replace("_", string.Empty);
        if (string.Equals(normalized, TargetLanguage, StringComparison.OrdinalIgnoreCase))
            return true;

        return candidate.IndexOf("english", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private sealed class LanguageCandidate
    {
        public object Item;
        public object Value;
        public int Index;
        public string Identifier;
    }
}

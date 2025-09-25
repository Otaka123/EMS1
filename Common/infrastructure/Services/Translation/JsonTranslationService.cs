using Common.Application.Contracts.interfaces;
using Common.Infrastructure.options;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.infrastructure.Services.Translation
{
    public class JsonTranslationService : ISharedMessageLocalizer
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _translationsCache = new();
        private readonly string _baseName;
        private readonly ILogger<JsonTranslationService> _logger;
        private readonly string _defaultCulture = "en";
        private readonly Assembly _assembly;

        public JsonTranslationService(
            IOptions<CustomLocalizationOptions> options,
            ILogger<JsonTranslationService> logger)
        {
            _baseName = options.Value.ResourcesBaseName ?? "messages";
            _logger = logger;
            _assembly = Assembly.GetExecutingAssembly(); // Assembly الخاص بـ Common

            _logger.LogInformation($"JsonTranslationService initialized for assembly: {_assembly.GetName().Name}");
            _logger.LogInformation($"Looking for resources with base name: {_baseName}");

            // تسجيل جميع Embedded Resources المتاحة لأغراض التصحيح
            var resourceNames = _assembly.GetManifestResourceNames();
            _logger.LogInformation($"Available embedded resources: {string.Join(", ", resourceNames)}");
        }

        // تنفيذ واجهة ISharedMessageLocalizer
        public void ReloadTranslations()
        {
            _translationsCache.Clear();
            _logger.LogInformation("Translations cache has been cleared");
        }

        public bool CultureExists(string cultureCode)
        {
            var resourceName = GetResourceName(cultureCode);
            return _assembly.GetManifestResourceStream(resourceName) != null;
        }

        public IEnumerable<string> GetAvailableCultures()
        {
            var prefix = $"Common.Application.Resources.{_baseName}.";
            var resourceNames = _assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (resourceName.StartsWith(prefix) && resourceName.EndsWith(".json"))
                {
                    var culture = resourceName
                        .Substring(prefix.Length)
                        .Replace(".json", "");
                    yield return culture;
                }
            }
        }

        public string GetTranslation(string key, string? defaultValue = null)
        {
            var value = GetString(key);
            return value ?? defaultValue ?? key;
        }

        public string GetTranslation(string key, string defaultValue, params object[] args)
        {
            var format = GetString(key);
            return string.Format(format ?? defaultValue ?? key, args);
        }

        // تنفيذ واجهة IStringLocalizer
        public LocalizedString this[string name] => GetLocalizedString(name);

        public LocalizedString this[string name, params object[] arguments] => GetLocalizedString(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var translations = LoadTranslations(culture);

            return translations.Select(t => new LocalizedString(t.Key, t.Value, false));
        }

        private LocalizedString GetLocalizedString(string name, params object[] arguments)
        {
            var value = GetString(name);
            var formattedValue = arguments.Length > 0 ?
                string.Format(value ?? name, arguments) :
                value ?? name;

            return new LocalizedString(name, formattedValue, resourceNotFound: value == null);
        }

        private string GetString(string key)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            var translations = LoadTranslations(culture);

            if (!translations.TryGetValue(key, out var value) && culture != _defaultCulture)
            {
                translations = LoadTranslations(_defaultCulture);
                translations.TryGetValue(key, out value);
            }

            return value;
        }

        private Dictionary<string, string> LoadTranslations(string culture)
        {
            return _translationsCache.GetOrAdd(culture, c =>
            {
                var resourceName = GetResourceName(c);

                try
                {
                    _logger.LogInformation($"Trying to load translations from embedded resource: {resourceName}");

                    using var stream = _assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                    {
                        _logger.LogWarning($"Embedded resource not found: {resourceName}");
                        return new Dictionary<string, string>();
                    }

                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        _logger.LogError($"Empty embedded resource: {resourceName}");
                        return new Dictionary<string, string>();
                    }

                    var resources = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    return FlattenDictionary(resources);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Critical error loading translations for culture {c} from resource {resourceName}");
                    return new Dictionary<string, string>();
                }
            });
        }

        private string GetResourceName(string culture)
        {
            // بناء اسم الـ resource بناءً على هيكل المشروع
            return $"Common.Application.Resources.{_baseName}.{culture}.json";
        }

        private Dictionary<string, string> FlattenDictionary(Dictionary<string, object> dictionary, string prefix = "")
        {
            var result = new Dictionary<string, string>();

            if (dictionary == null)
            {
                _logger.LogWarning("Attempted to flatten a null dictionary");
                return result;
            }

            foreach (var item in dictionary)
            {
                var fullKey = string.IsNullOrEmpty(prefix) ? item.Key : $"{prefix}.{item.Key}";

                try
                {
                    switch (item.Value)
                    {
                        case JsonElement element when element.ValueKind == JsonValueKind.Object:
                            var nested = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                            var nestedFlattened = FlattenDictionary(nested, fullKey);
                            foreach (var nestedItem in nestedFlattened)
                            {
                                result[nestedItem.Key] = nestedItem.Value;
                            }
                            break;

                        case JsonElement element:
                            result[fullKey] = element.ToString();
                            break;

                        case Dictionary<string, object> nestedDict:
                            var dictFlattened = FlattenDictionary(nestedDict, fullKey);
                            foreach (var dictItem in dictFlattened)
                            {
                                result[dictItem.Key] = dictItem.Value;
                            }
                            break;

                        case null:
                            result[fullKey] = string.Empty;
                            break;

                        default:
                            result[fullKey] = item.Value.ToString();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error flattening dictionary item with key '{Key}'", fullKey);
                    result[fullKey] = string.Empty;
                }
            }

            return result;
        }
    }
}
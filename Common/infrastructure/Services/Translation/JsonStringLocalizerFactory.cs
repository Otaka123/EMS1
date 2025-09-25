using Common.Application.Contracts.interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.infrastructure.Services.Translation
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ISharedMessageLocalizer _sharedLocalizer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache = new();
        
        public JsonStringLocalizerFactory(
            ISharedMessageLocalizer sharedLocalizer,
            ILoggerFactory loggerFactory)
        {
            _sharedLocalizer = sharedLocalizer ?? throw new ArgumentNullException(nameof(sharedLocalizer));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }

            var typeName = resourceSource.Name;
            return _localizerCache.GetOrAdd(typeName, _ =>
                new JsonStringLocalizer(
                    _sharedLocalizer,
                    _loggerFactory.CreateLogger<JsonStringLocalizer>(),
                    typeName));
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            return _localizerCache.GetOrAdd(baseName, _ =>
                new JsonStringLocalizer(
                    _sharedLocalizer,
                    _loggerFactory.CreateLogger<JsonStringLocalizer>(),
                    baseName));
        }
    }

    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ISharedMessageLocalizer _sharedLocalizer;
        private readonly ILogger _logger;
        private readonly string _contextName;

        public JsonStringLocalizer(
            ISharedMessageLocalizer sharedLocalizer,
            ILogger logger,
            string contextName)
        {
            _sharedLocalizer = sharedLocalizer ?? throw new ArgumentNullException(nameof(sharedLocalizer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextName = contextName;
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = _sharedLocalizer[name];
                _logger.LogTrace($"Localizing key '{name}' in context '{_contextName}'. Found: {!value.ResourceNotFound}");
                return value;
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = _sharedLocalizer[name, arguments];
                _logger.LogTrace($"Localizing key '{name}' with args in context '{_contextName}'. Found: {!value.ResourceNotFound}");
                return value;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            _logger.LogTrace($"Getting all strings for context '{_contextName}'");
            return _sharedLocalizer.GetAllStrings(includeParentCultures);
        }
    }
}

using Common.Application.Common;
using Common.Application.Contracts.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Common.Application.Contracts.interfaces;

namespace Common.Infrastructure.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidationService> _logger;
        private readonly ConcurrentDictionary<Type, object> _validatorCache = new();
        private readonly ISharedMessageLocalizer _localizer;

        public ValidationService(
            IServiceProvider serviceProvider,
            ILogger<ValidationService> logger,
            ISharedMessageLocalizer localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                if (instance == null)
                {
                    _logger.LogWarning("Validation attempted on null instance of type {Type}", typeof(T).Name);
                    return new ValidationResult(new[] { new ValidationFailure("", "Instance cannot be null") });
                }

                var validator = GetCachedValidator<T>();
                return await validator.ValidateAsync(new ValidationContext<T>(instance), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during validation of type {Type}", typeof(T).Name);
                return new ValidationResult(new[] { new ValidationFailure("", $"Validation error: {ex.Message}") });
            }
        }


        //public ResponseError CreateValidationError(IEnumerable<ValidationFailure> failures)
        //{
        //    var errorMessages = failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}");
        //    return ResponseError.ValidationError.WithDetails(errorMessages.ToArray());
        //}
        public ResponseError CreateValidationError(IEnumerable<ValidationFailure> errors)
        {
            return ResponseError.Create(
                "ValidationError",
                _localizer["ValidationError"],
                errors.ToDictionary(
                    e => e.PropertyName,
                    e => new[] { _localizer[e.ErrorMessage] ?? e.ErrorMessage }));
        }

        //private IValidator<T> GetCachedValidator<T>() where T : class
        //{
        //    return (IValidator<T>)_validatorCache.GetOrAdd(
        //        typeof(T),
        //        t => _serviceProvider.GetService<IValidator<T>>()
        //             ?? throw new Exception($"Validator not found for type {typeof(T).Name}"));
        //}

        private IValidator<T> GetCachedValidator<T>() where T : class
        {
            return (IValidator<T>)_validatorCache.GetOrAdd(
                typeof(T),
                t => _serviceProvider.GetService<IValidator<T>>() ?? new InlineValidator<T>()); // ✅ تعديل هنا
        }



        public async Task<bool> IsValidAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class
        {
            var result = await ValidateAsync(instance, cancellationToken);
            return result.IsValid;
        }

        public async Task<IEnumerable<string>> GetValidationMessagesAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class
        {
            var result = await ValidateAsync(instance, cancellationToken);
            return result.Errors.Select(e => e.ErrorMessage);
        }

        public async Task EnsureValidAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class
        {
            var result = await ValidateAsync(instance, cancellationToken);
            if (!result.IsValid)
            {
                throw new System.Exception("Validation failed: " +
                    string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
            }
        }
        public async Task<ValidationResult> ValidatePropertiesAsync<T>(
            T instance,
            IEnumerable<string> propertyNames,
            CancellationToken cancellationToken = default) where T : class
        {
            if (instance == null)
                return new ValidationResult(new[] { new ValidationFailure("", "Instance cannot be null") });

            if (propertyNames?.Any() != true)
                return await ValidateAsync(instance, cancellationToken);

            try
            {
                var validator = GetCachedValidator<T>();
                var failures = new List<ValidationFailure>();

                foreach (var propertyName in propertyNames)
                {
                    var context = new ValidationContext<T>(instance);
                    context.RootContextData["__FV_IncludeProperties"] = new[] { propertyName };

                    var result = await validator.ValidateAsync(context, cancellationToken);
                    failures.AddRange(result.Errors);
                }

                return new ValidationResult(failures);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Property validation error");
                return new ValidationResult(new[] { new ValidationFailure("", $"Property validation error: {ex.Message}") });
            }
        }

    }
}
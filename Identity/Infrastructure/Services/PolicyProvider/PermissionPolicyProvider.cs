using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Services.PolicyProvider
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, AuthorizationPolicy> _policyCache;

        public PermissionPolicyProvider(
            IServiceProvider serviceProvider,
            Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            _serviceProvider = serviceProvider;
            _policyCache = new ConcurrentDictionary<string, AuthorizationPolicy>();
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
            _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // التحقق من الذاكرة المؤقتة أولاً للأداء
            if (_policyCache.TryGetValue(policyName, out var cachedPolicy))
            {
                return cachedPolicy;
            }

            // تقسيم policyName والتحقق من التنسيق
            var parts = policyName.Split(':');
            if (parts.Length != 3)
            {
                return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            var category = parts[0];
            var permissionType = parts[1];
            var name = parts[2];

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

            // البحث باستخدام الأعمدة المنفردة (يمكن ترجمته إلى SQL بكفاءة)
            var permissionExists = await dbContext.Permissions
                .AnyAsync(p => p.Category == category &&
                              p.PermissionType == permissionType &&
                              p.Name == name);

            if (permissionExists)
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireClaim("Permission", policyName)
                    .Build();

                // تخزين في الذاكرة المؤقتة للأداء
                _policyCache.TryAdd(policyName, policy);
                return policy;
            }

            return await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
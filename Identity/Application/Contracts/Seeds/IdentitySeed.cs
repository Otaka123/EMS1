using Google;
using Identity.Application.Contracts.Enum;
using Identity.Domain;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Seeds
{
    public static class DataSeeder
    {
        public static async Task SeedAdminUserAndPermissions(AppIdentityDbContext context, UserManager<AppUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            await SeedPermissions(context);
            await SeedRoles(context, roleManager);
            await SeedAdminUser(context, userManager);
            await SeedRegularUser(context, userManager); // إضافة المستخدم العادي
        }

        private static async Task SeedRoles(AppIdentityDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            await SeedAdminRole(context, roleManager);
            await SeedUserRole(context, roleManager); // إضافة دور المستخدم العادي
        }

        private static async Task SeedUserRole(AppIdentityDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            var userRoleName = "User";

            var userRole = await roleManager.FindByNameAsync(userRoleName);
            if (userRole == null)
            {
                userRole = new ApplicationRole
                {
                    Name = userRoleName,
                    NormalizedName = userRoleName.ToUpper()
                };

                await roleManager.CreateAsync(userRole);
            }

            // إضافة صلاحيات أساسية للمستخدم العادي
            var basicPermissions = await context.Permissions
                .Where(p => p.IsActive &&
                           (p.Name.Contains("View") ||
                            p.Category == "Reporting" ||
                            p.Name == "Users.Edit")) // صلاحيات عرضية أساسية
                .ToListAsync();

            // Remove existing permissions for the role
            var existingRolePermissions = context.RolePermissions.Where(rp => rp.RoleId == userRole.Id);
            context.RolePermissions.RemoveRange(existingRolePermissions);

            // Add basic permissions to User role
            foreach (var permission in basicPermissions)
            {
                var rolePermission = new RolePermission(userRole.Id, permission.Id);
                context.RolePermissions.Add(rolePermission);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedAdminRole(AppIdentityDbContext context, RoleManager<ApplicationRole> roleManager)
        {
            var adminRoleName = "Admin";

            var adminRole = await roleManager.FindByNameAsync(adminRoleName);
            if (adminRole == null)
            {
                adminRole = new ApplicationRole
                {
                    Name = adminRoleName,
                    NormalizedName = adminRoleName.ToUpper()
                };

                await roleManager.CreateAsync(adminRole);
            }

            // Get all permissions
            var allPermissions = await context.Permissions.Where(p => p.IsActive).ToListAsync();

            // Remove existing permissions for the role
            var existingRolePermissions = context.RolePermissions.Where(rp => rp.RoleId == adminRole.Id);
            context.RolePermissions.RemoveRange(existingRolePermissions);

            // Add all permissions to Admin role
            foreach (var permission in allPermissions)
            {
                var rolePermission = new RolePermission(adminRole.Id, permission.Id);
                context.RolePermissions.Add(rolePermission);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedAdminUser(AppIdentityDbContext context, UserManager<AppUser> userManager)
        {
            var adminEmail = "admin@example.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = GenderType.Male,
                    DOB = new DateTime(1980, 1, 1)
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    await userManager.AddClaimAsync(adminUser, new Claim("FullName", "System Administrator"));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedRegularUser(AppIdentityDbContext context, UserManager<AppUser> userManager)
        {
            var userEmail = "user@example.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);

            if (regularUser == null)
            {
                regularUser = new AppUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = "Ahmed",
                    LastName = "Mohamed",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = GenderType.Male,
                    DOB = new DateTime(1990, 5, 15),
                    Address = "123 Main Street, Cairo, Egypt",
                    ProfilePictureUrl = "/images/users/user-profile.jpg"
                };

                var result = await userManager.CreateAsync(regularUser, "User123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                    await userManager.AddClaimAsync(regularUser, new Claim("FullName", "Ahmed Mohamed"));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(regularUser, "User"))
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }

        // يمكنك إضافة المزيد من المستخدمين إذا أردت
        private static async Task SeedAdditionalUsers(AppIdentityDbContext context, UserManager<AppUser> userManager)
        {
            var users = new[]
            {
            new {
                Email = "sara@example.com",
                FirstName = "Sara",
                LastName = "Ali",
                Password = "Sara123!",
                Gender = GenderType.Female,
                DOB = new DateTime(1995, 8, 20)
            },
            new {
                Email = "mohamed@example.com",
                FirstName = "Mohamed",
                LastName = "Hassan",
                Password = "Mohamed123!",
                Gender = GenderType.Male,
                DOB = new DateTime(1988, 3, 10)
            }
        };

            foreach (var userInfo in users)
            {
                var user = await userManager.FindByEmailAsync(userInfo.Email);
                if (user == null)
                {
                    user = new AppUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Gender = userInfo.Gender,
                        DOB = userInfo.DOB
                    };

                    var result = await userManager.CreateAsync(user, userInfo.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
        }

        private static async Task SeedPermissions(AppIdentityDbContext context)
        {
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
            {
                // User Management Permissions (للمستخدم العادي - عرض فقط)
                new Permission("Users.View", "View Users", "UserManagement", "Read"),
                new Permission("Users.Create", "Create Users", "UserManagement", "Write"),
                new Permission("Users.Edit", "Edit Users", "UserManagement", "Write"),
                new Permission("Users.Delete", "Delete Users", "UserManagement", "Write"),

                // Profile Management (للمستخدم العادي)
                new Permission("Profile.View", "View Profile", "ProfileManagement", "Read"),
                new Permission("Profile.Edit", "Edit Profile", "ProfileManagement", "Write"),
                new Permission("Profile.ChangePassword", "Change Password", "ProfileManagement", "Write"),

                // Role Management Permissions (للمشرفين فقط)
                new Permission("Roles.View", "View Roles", "RoleManagement", "Read"),
                new Permission("Roles.Create", "Create Roles", "RoleManagement", "Write"),
                new Permission("Roles.Edit", "Edit Roles", "RoleManagement", "Write"),
                new Permission("Roles.Delete", "Delete Roles", "RoleManagement", "Write"),

                // Permission Management (للمشرفين فقط)
                new Permission("Permissions.View", "View Permissions", "PermissionManagement", "Read"),
                new Permission("Permissions.Manage", "Manage Permissions", "PermissionManagement", "Write"),

                // System Settings (للمشرفين فقط)
                new Permission("Settings.View", "View Settings", "SystemSettings", "Read"),
                new Permission("Settings.Edit", "Edit Settings", "SystemSettings", "Write"),

                // Reports (للجميع)
                new Permission("Reports.View", "View Reports", "Reporting", "Read"),
                new Permission("Reports.Generate", "Generate Reports", "Reporting", "Write"),

                // Audit Logs (للمشرفين فقط)
                new Permission("AuditLogs.View", "View Audit Logs", "Auditing", "Read")
            };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }

}

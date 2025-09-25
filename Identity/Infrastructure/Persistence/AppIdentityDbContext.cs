using Identity.Application.Contracts.Enum;
using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence
{
    public class AppIdentityDbContext: IdentityDbContext<AppUser, ApplicationRole, string>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<SystemHistory> SystemHistories { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        // ✅ هنا التعديل
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعادة تسمية جداول Identity
            modelBuilder.Entity<AppUser>().ToTable("Users");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

            // إعداد AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.Property(e => e.Gender)
                    .HasConversion<byte>()
                    .HasDefaultValue(GenderType.Unknown)
                    .HasComment("0=Unknown, 1=Male, 2=Female, 3=Other, 4=PreferNotToSay");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(255);
                entity.Property(e => e.Address).HasMaxLength(200);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.HasIndex(e => new { e.LastName, e.FirstName });

                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.DOB).HasColumnType("date");
            });

            // إعداد ApplicationRole
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.NormalizedName).HasMaxLength(100);
                entity.HasIndex(e => e.NormalizedName).IsUnique();
            });

            // إعداد Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
                entity.Property(e => e.DisplayName).HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PermissionType).HasMaxLength(100);
            });

            // إعداد PermissionHistory
            modelBuilder.Entity<SystemHistory>(entity =>
            {
                entity.Property(e => e.ChangedAt)
                    .HasDefaultValueSql("GETUTCDATE()")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ChangedByUserId).HasMaxLength(100).IsRequired();

            });

            // إعداد RolePermission (علاقة many-to-many بين Role و Permission)
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // إعداد RefreshToken
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

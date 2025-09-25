
//using Identity.API.Extensions;
//using Identity.Application.Contracts.Seeds;
//using Identity.Domain;
//using Identity.Infrastructure.Middleware;
//using Identity.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Identity;

//var builder = WebApplication.CreateBuilder(args);



////builder.Services.AddControllers();
//builder.Services.AddControllersWithViews() ;
//builder.Services.AddRazorPages();

//builder.Services.AddSwaggerDocumentation();
//builder.Services.AddIdentityAndAuthentication(builder.Configuration);
//builder.Services.AddLocalizationSupport(builder.Configuration);

//var app = builder.Build();

//// Seed Roles & Permissions
////await app.Services.SeedIdentityDataAsync();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwaggerDocumentation();
//}

//app.UseHttpsRedirection();

//app.UseStaticFiles(); // لو هتستخدم MVC Views أو Areas

//app.UseRouting();

//app.UseCors("AllowMvcApp");

//app.UseAuthentication();
//app.UseMiddleware<JwtRefreshMiddleware>();
//app.UseAuthorization();
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    try
//    {
//        var context = services.GetRequiredService<AppIdentityDbContext>();
//        var userManager = services.GetRequiredService<UserManager<AppUser>>();
//        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

//        // Run Seeders
//        await DataSeeder.SeedAdminUserAndPermissions(context, userManager, roleManager);
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "Error occurred while seeding the database.");
//    }
//}
//app.UseLocalizationSupport();

//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
//);

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

////app.MapControllers();

//app.Run();
using Identity.API.Extensions;
using Identity.Application.Contracts.Seeds;
using Identity.Domain;
using Identity.Infrastructure.Middleware;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// إضافة خدمات MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSwaggerDocumentation();
builder.Services.AddIdentityAndAuthentication(builder.Configuration);
builder.Services.AddLocalizationSupport(builder.Configuration);

var app = builder.Build();

// Seed Roles & Permissions
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(); // Swagger متاح فقط في البيئة التطويرية
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ضروري للملفات الثابتة في MVC
app.UseRouting();
app.UseCors("AllowMvcApp");
app.UseMiddleware<Identity.Infrastructure.Middleware.JwtRefreshMiddleware>();

app.UseAuthentication();
app.UseMiddleware<JwtRefreshMiddleware>();
app.UseAuthorization();

// Seed البيانات
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppIdentityDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        await DataSeeder.SeedAdminUserAndPermissions(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error occurred while seeding the database.");
    }
}

app.UseLocalizationSupport();

// تعيين الـ Routes - يجب أن تكون Area Route أولاً
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");

app.Run();
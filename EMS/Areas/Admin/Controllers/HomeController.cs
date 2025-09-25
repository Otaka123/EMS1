using Common.Application.Contracts.interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;

namespace Identity.API.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {


            // التحقق هل المستخدم مسجل دخول ولا لأ
            if (User.Identity.IsAuthenticated)
            {
                if (Request.Cookies.TryGetValue("JWT_TOKEN", out var token) && !string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var roles = jwt.Claims
                                          .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                                          .Select(c => c.Value)
                                          .ToList();

                    // لو معاه دور Admin
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });
                    }
                }

                // جلب جميع الأدوار الخاصة بالمستخدم


                // لو مستخدم عادي دخله على الصفحة العادية
                return View();
            }


            return View();

        }
    }
}

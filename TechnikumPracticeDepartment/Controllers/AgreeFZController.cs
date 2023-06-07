using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TechnikumPracticeDepartment.Controllers
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class AgreeFZController : Controller
    {

        TechnikumPracticeDepartmentContext db;
        public AgreeFZController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public IActionResult AgreeFZ()
        {
            if (!User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("Index", "Home");
            var EmailUser = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = db.Users.Where(p => p.Email == EmailUser).FirstOrDefault();
            return View("~/Views/AgreeFZ.cshtml", user);
        }
        public async Task<IActionResult> AgreeFZTrueAsync()
        {
            if (!User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("Index", "Home");
            var EmailUser = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            var user = db.Users.Where(p => p.Email == EmailUser).FirstOrDefault();
            user.Fz152 = true;
            db.Users.Update(user);
            db.SaveChanges();

            var claimsIdentity = new ClaimsIdentity("Application");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, user.SurnameUser + " " + user.NameUser));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            var rolesUser = db.UsersRoles.Where(p => p.UserId == user.IdUser).ToList();
            if (user.Fz152 != true)
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Не подтверждён ФЗ"));
            else
            {
                if (rolesUser.Where(p => p.RoleId == 1).Count() > 0)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Администратор"));
                if (rolesUser.Where(p => p.RoleId == 2).Count() > 0)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Сотрудник производственного отдела"));
                if (rolesUser.Where(p => p.RoleId == 3).Count() > 0)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Работадатель"));
                if (rolesUser.Where(p => p.RoleId == 4).Count() > 0)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Студент"));
            }
            await HttpContext.SignOutAsync("Application");
            await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
    }
}

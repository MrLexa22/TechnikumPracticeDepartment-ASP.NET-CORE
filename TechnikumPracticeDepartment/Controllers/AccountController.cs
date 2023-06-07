using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using Microsoft.EntityFrameworkCore;

namespace TechnikumPracticeDepartment.Controllers
{
    public class AccountController : Controller
    {
        private IConfiguration configuration;
        private TechnikumPracticeDepartmentContext db;
        public AccountController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
        }
        public IActionResult Login(string returnUrl)
        {
            return new ChallengeResult(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
                });
        }
        public async Task<IActionResult> LoginWithLoginAsync(ModelErrorWindow model)
        {
            if (model.Login == null || model.Login == "")
                return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Логин указан некорректно", ErrorTitle = "Ошибка авторизации" });
            if (model.Password == null || model.Password == "")
                return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Пароль указан некорректно", ErrorTitle = "Ошибка авторизации" });

            int find = 0;
            try
            {
                find = db.Users.Where(p => p.Email == model.Login).Count();
            }
            catch { }
            if (find <= 0)
                return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Пользователь с таким логином не найден", ErrorTitle = "Ошибка авторизации" });

            var userDB = db.Users.Where(p => p.Email == model.Login).Single();
            if (userDB.Password != HashPassword.hashPassword(model.Password))
                return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Вы ввели некорректный пароль", ErrorTitle = "Ошибка авторизации" });

            if(userDB.IsAvaliable == false)
                return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Ваш аккаунт заблокирован. Обратитесь к сотруднику производственного отдела", ErrorTitle = "Ошибка авторизации" });
            
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();
            if (rolesUser.Where(p => p.RoleId == 4).Count() > 0)
            {
                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                var student = db.Students.Include(p=>p.Group).Where(p=>p.UserId == userDB.IdUser).First();
                bool IsEnded = (nowDate > augustDate) ? (student.Group.YearOfGraduation <= yearNow ? true : false) : (student.Group.YearOfGraduation < yearNow ? true : false);
                if(IsEnded == true)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Обучение вашей группы завершено", ErrorTitle = "Ошибка авторизации" });
                if(student.IsStudent == false)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Вы не числитесь в контингенте ОУ. Обратитесь к администратору", ErrorTitle = "Ошибка авторизации" });
            }
            if(rolesUser.Where(p => p.RoleId == 3).Count() > 0)
            {
                var organization = db.EmployeeOfOrganizations.Include(p=>p.Organization).Where(p => p.UserId == userDB.IdUser).First();
                if(organization.Organization.IsAvaliable == false)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { Login = model.Login, Password = model.Password, IsError = true, ErrorMessage = "Ваша организация не может искать студентов для поиска прохождения практики. Обратитесь к администратору", ErrorTitle = "Ошибка авторизации" });
            }

            var claimsIdentity = new ClaimsIdentity("Application");
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userDB.SurnameUser + " " + userDB.NameUser));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, userDB.Email));
            if (userDB.Fz152 != true)
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

            await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Application");
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("External");

            string email = authenticateResult.Principal.FindFirst(ClaimTypes.Email).Value.ToString();
            string domen = email.Substring(email.LastIndexOf('@') + 1);
            if (domen != "mpt.ru")
            {
                return RedirectToAction("Index", "Home", new ModelErrorWindow { IsError = true, ErrorMessage = "Авторизация через Google возможна лишь с использованием почты в домене mpt.ru", ErrorTitle = "Ошибка авторизации" });
            }
            else
            {
                if (db.Users.Where(p => p.Email == authenticateResult.Principal.FindFirst(ClaimTypes.Email).Value.ToString()).Count() <= 0)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { IsError = true, ErrorMessage = "Пользователь в системе с таким Email не найден!", ErrorTitle = "Ошибка авторизации" });
                Debug.WriteLine("Access!");
            }

            if (!authenticateResult.Succeeded)
                return BadRequest(); // TODO: Handle this better.

            var claimsIdentity = new ClaimsIdentity("Application");

            var userDB = db.Users.Where(p => p.Email == authenticateResult.Principal.FindFirst(ClaimTypes.Email).Value).First();
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();
            if (rolesUser.Where(p => p.RoleId == 4).Count() > 0)
            {
                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                var student = db.Students.Include(p => p.Group).Where(p => p.UserId == userDB.IdUser).First();
                bool IsEnded = (nowDate > augustDate) ? (student.Group.YearOfGraduation <= yearNow ? true : false) : (student.Group.YearOfGraduation < yearNow ? true : false);
                if (IsEnded == true)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { IsError = true, ErrorMessage = "Обучение вашей группы завершено", ErrorTitle = "Ошибка авторизации" });
                if (student.IsStudent == false)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { IsError = true, ErrorMessage = "Вы не числитесь в контингенте ОУ. Обратитесь к администратору", ErrorTitle = "Ошибка авторизации" });
            }
            if (rolesUser.Where(p => p.RoleId == 3).Count() > 0)
            {
                var organization = db.EmployeeOfOrganizations.Include(p => p.Organization).Where(p => p.UserId == userDB.IdUser).First();
                if (organization.Organization.IsAvaliable == false)
                    return RedirectToAction("Index", "Home", new ModelErrorWindow { IsError = true, ErrorMessage = "Ваша организация не может искать студентов для поиска прохождения практики. Обратитесь к администратору", ErrorTitle = "Ошибка авторизации" });
            }
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, userDB.SurnameUser + " " + userDB.NameUser));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, userDB.Email));
            if (userDB.Fz152 != true)
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

            await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> ForgotPassword()
        {
            return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = false });
        }
        
        [HttpPost]
        public async Task<IActionResult> ForgotPasswordPost1(ModelErrorWindow model)
        {
            if(model.Login == null)
                return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = false, IsError = true, ErrorTitle = "Ошибка", ErrorMessage = "Вы не указали email для восстановления пароля" });
            if (model.Login.Trim() == "")
                return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = false, IsError = true, ErrorTitle = "Ошибка", ErrorMessage = "Вы не указали email для восстановления пароля" });
            
            if(db.Users.Where(p=>p.Email == model.Login).Count() <= 0)
                return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = false, IsError = true, ErrorTitle = "Ошибка", ErrorMessage = "Указанный логин не существует" });
            else if(db.Users.Where(p => p.Email == model.Login).First().IsAvaliable == false)
                return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = false, IsError = true, ErrorTitle = "Ошибка", ErrorMessage = "Указанный аккаунт заблокирован" });
            
            Random rnd = new Random();
            int code = rnd.Next(10000, 99999);
            Response.Cookies.Append("Code", HashPassword.hashPassword(code.ToString()));
            _ = new EmailService(configuration).SendEmailRecovertPassword(code, model.Login);
            
            return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = true, Login = model.Login });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordPost2(ModelErrorWindow model)
        {
            if (HashPassword.hashPassword(model.Password) != Request.Cookies["Code"])
                return View("~/Views/Home/RecoveryPassword.cshtml", new ModelErrorWindow { IsRecovery = true, Login = model.Login, IsError = true, ErrorTitle = "Ошибка", ErrorMessage = "Вы ввели некорректный код с почты" });
            Response.Cookies.Delete("Code");
            var user = db.Users.Where(p => p.Email == model.Login).First();
            string password = HashPassword.GetRandomPassword(8);
            user.Password = HashPassword.hashPassword(password);
            db.Users.Update(user);
            db.SaveChanges();
            _ = new EmailService(configuration).SendEmailRegistrationAsync(model.Login, password, 2);

            return RedirectToAction("Index","Home",new ModelErrorWindow { IsError = true, ErrorMessage = "Ваш пароль был спрошен и новый направлен на почту", ErrorTitle = "Новый пароль направлен на почту"});
        }
    }
}

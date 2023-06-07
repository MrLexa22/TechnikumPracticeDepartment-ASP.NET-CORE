using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechnikumPracticeDepartment.ModelsDB;
using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace TechnikumPracticeDepartment.Controllers
{
    public class AdditionalController : Controller
    {
        private static string host = "89.108.64.223";
        private static string username = "root";
        private static string password = "$gMkW9wm";

        TechnikumPracticeDepartmentContext db;
        public AdditionalController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public bool UpdateIn(int role)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return false;
            if (!User.Identity.IsAuthenticated)
                return false;

            User userDB = new User();
            try
            {
                userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                if (userDB == null)
                {
                    HttpContext.SignOutAsync("Application");
                    return false;
                }
            }
            catch
            {
                return false;
            }

            if (role == 0)
                return true;
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();
            if (role > 0 && rolesUser.Where(p => p.RoleId == role).Count() > 0)
                return true;
            else
                return false;
        }
        public IActionResult LeftMenu(int index)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return PartialView("~/Views/ManageAdmin/_LeftMenu.cshtml", index);
        }
        public IActionResult LeftMenuPractice(int index)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return PartialView("~/Views/ManageAdmin/_LeftMenuPractice.cshtml", index);
        }
        public IActionResult LeftMenuOrganization(int index)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");
            return PartialView("~/Views/OrganizationPage/_LeftMenu.cshtml", index);
        }
        public IActionResult LeftMenuStudent(int index)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            return PartialView("~/Views/StudentPage/_LeftMenu.cshtml", index);
        }
        public ActionResult GetImageFromServer(string path)
        {
            if (UpdateIn(0) == false)
                return null;
            try
            {
                var connectionInfo = new ConnectionInfo(host, "root", new PasswordAuthenticationMethod(username, password));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    var bytesImage = sftp.ReadAllBytes($@"/var/www/ElStudent/Students/{path}");
                    sftp.Disconnect();
                    return File(bytesImage, "image/jpg");
                }
            }
            catch
            {
                var connectionInfo = new ConnectionInfo(host, "root", new PasswordAuthenticationMethod(username, password));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    var bytesImage = sftp.ReadAllBytes($@"/var/www/ElStudent/NotFound.jpg");
                    sftp.Disconnect();
                    return File(bytesImage, "image/jpg");
                }
            }
        }
        public ActionResult GetPDFFromServer(string path)
        {
            if (UpdateIn(0) == false)
                return null;
            try
            {
                var connectionInfo = new ConnectionInfo(host, "root", new PasswordAuthenticationMethod(username, password));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    var bytesImage = sftp.ReadAllBytes($@"/var/www/ElStudent/Students/{path}");
                    sftp.Disconnect();
                    return File(bytesImage, "application/pdf");
                }
            }
            catch
            { return null; }
        }
    }
}

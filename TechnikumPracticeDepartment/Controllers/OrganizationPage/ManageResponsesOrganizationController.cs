using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsManageResponses;
using TechnikumPracticeDepartment.Models.ModelsStudents;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Controllers.OrganizationPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class ManageResponsesOrganizationController : Controller
    {
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
        
        private TechnikumPracticeDepartmentContext db;
        public ManageResponsesOrganizationController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/OrganizationPage/Responses/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? sortList, int? filterListType, int? filterListStatus, string search)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");
            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
            
            IndexStudentsResponses model = new();

            model.responses_fromStudent = new();
            model.responses_fromOrganization = new();
            model.responses_fromStudent = db.ResponseFromStudents.Include(p=>p.Student).ThenInclude(p=>p.Resume).Include(p=>p.Student).ThenInclude(p=>p.Group).ThenInclude(p=>p.Specialization).Include(p => p.Vacancy).ThenInclude(p => p.Organization).Where(p => p.Vacancy.OrganizationId == organization.IdOrganization).ToList();
            model.responses_fromOrganization = db.ResponseFromOrganizations.Include(p => p.Vacancy).ThenInclude(p => p.Organization).Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p=>p.Group).ThenInclude(p=>p.Specialization).Where(p => p.Vacancy.OrganizationId == organization.IdOrganization).ToList();

            if (sortList > 0)
            {
                model.responses_fromStudent.OrderBy(p => p.DateTimeCreate).ToList();
                model.responses_fromOrganization.OrderBy(p => p.DateTimeCreate).ToList();
            }

            if (filterListType == 1)
                model.responses_fromOrganization = new();
            if (filterListType == 2)
                model.responses_fromStudent = new();

            if (filterListStatus > -1)
            {
                model.responses_fromStudent = model.responses_fromStudent.Where(p => p.Status == filterListStatus).ToList();
                model.responses_fromOrganization = model.responses_fromOrganization.Where(p => p.Status == filterListStatus).ToList();
            }

            if (!String.IsNullOrEmpty(search))
            {
                model.responses_fromStudent = model.responses_fromStudent.Where(p => p.Vacancy.NameVacancy.ToLower().Contains(search.ToLower())).ToList();
                model.responses_fromOrganization = model.responses_fromOrganization.Where(p => p.Vacancy.NameVacancy.ToLower().Contains(search.ToLower())).ToList();
            }

            return PartialView("~/Views/OrganizationPage/Responses/_ResponsesList.cshtml", model);
        }
    }
}

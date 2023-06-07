using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Configuration;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.Models.ModelsOrganizationPages;
using TechnikumPracticeDepartment.Models.ModelsResumeStudent;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Controllers.OrganizationPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class ResumeStudentsController : Controller
    {
        public int GetAge(DateOnly? dateOfBirthday)
        {
            if (dateOfBirthday == null)
                return -1;

            DateOnly date = (DateOnly)dateOfBirthday;
            DateTime birthdate = date.ToDateTime(new TimeOnly(0, 0, 0));
            var today = DateTime.Today;
            var age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age)) 
                age--;
            return age;
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
        private TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public ResumeStudentsController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
        }
        public IActionResult Index()
        {
            if (UpdateIn(3) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/OrganizationPage/ResumeStudents/Index.cshtml", db.Specializations.ToList());
        }
        public IActionResult GetList(int? typeSortList, int? specializationId, int? course, string? filterTags, string search, int page = 1)
        {
            if (UpdateIn(3) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            IndexResumeStudents model = new();

            int pageSize = 15;
            model.list_resume = new();

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);

            var resumes = db.Resumes.Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).Where(p=>p.IsAvaliable == true || p.IsAvaliable == null).ToList();
            var listStudents = resumes.Select(a => new Students()
            {
                student = a.Student,
                ID_Group = a.Student.Group.IdGroup,
                NameGroups = a.Student.Group.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Student.Group.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation + 1)
                                        : (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation),
                SpecializationName = a.Student.Group.Specialization.SpecializationCode,
                Specialization_ID = a.Student.Group.SpecializationId,
                YearStartEducation = a.Student.Group.YearStartEducation,
                YearOfGraduation = a.Student.Group.YearOfGraduation,
                age = GetAge(a.Student.DateOfBirthday)
            }).ToList();
            listStudents = listStudents.Where(p => p.IsEnded == false).ToList();
            listStudents = listStudents.Where(p => p.student.Resume.IsAvaliable == null || p.student.Resume.IsAvaliable == true).ToList();

            if (typeSortList == 1)
            {
                listStudents = listStudents.OrderBy(p => p.student.Resume.DesiredPosition).ToList();
            }
            if (!String.IsNullOrEmpty(search))
            {
                listStudents = listStudents.Where(p => p.student.Resume.DesiredPosition.ToLower().Contains(search.ToLower())).ToList();
            }
            if (filterTags != null)
            {
                listStudents = listStudents.Where(p => p.student.Resume.TagsSkills.ToLower().Contains(filterTags.ToLower())).ToList();
            }
            if(course > 0)
            {
                listStudents = listStudents.Where(p => p.Course == course).ToList();
            }
            if(specializationId > 0)
            {
                listStudents = listStudents.Where(p => p.Specialization_ID == specializationId).ToList();
            }

            var items = listStudents.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = listStudents.Count();
            model.list_resume = listStudents;
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_ResumeStudents(typeSortList, course, specializationId, filterTags, search);

            return PartialView("~/Views/OrganizationPage/ResumeStudents/_ListResumeStudents.cshtml", model);
        }
        public IActionResult LookResume(int IdStudent)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");

            ResumeModel resumeModel = new ResumeModel();

            var resume = db.Students.Include(p=>p.Resume).Include(p=>p.User).Include(p=>p.Group).ThenInclude(p=>p.Specialization).Where(p => p.IdStudent == IdStudent).First();
            
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            int Course = (nowDate > augustDate) ? (resume.Group.YearOfGraduation <= yearNow ? resume.Group.YearOfGraduation - resume.Group.YearStartEducation : yearNow - resume.Group.YearStartEducation + 1)
                                : (resume.Group.YearOfGraduation <= yearNow ? resume.Group.YearOfGraduation - resume.Group.YearStartEducation : yearNow - resume.Group.YearStartEducation);

            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            var _ResponseStud = db.ResponseFromStudents.Include(p=>p.Vacancy).ThenInclude(p=>p.Organization).Where(p => p.StudentId == resume.IdStudent && p.Vacancy.Organization.IdOrganization == organization.IdOrganization && p.Status != 4 && p.Status != 5 && p.Status != 6).ToList();
            var _ResponseOrg = db.ResponseFromOrganizations.Include(p=>p.Vacancy).Include(p=>p.Resume).Where(p=>p.Vacancy.OrganizationId == organization.IdOrganization && p.Resume.StudentId == resume.IdStudent && p.Status != 4 && p.Status != 5 && p.Status != 6).ToList();

            resumeModel.responseFromStudent = new();
            resumeModel.responseFromOrganization = new();
            resumeModel.vacancies = new();
            resumeModel.vacancies = db.Vacancies.Where(p => p.OrganizationId == organization.IdOrganization).ToList();

            if (_ResponseStud.Count > 0 )
                resumeModel.responseFromStudent = _ResponseStud.ToList();
            if (_ResponseOrg.Count > 0)
                resumeModel.responseFromOrganization = _ResponseOrg.ToList();

            resumeModel.course = Course.ToString();
            resumeModel.student_info = resume;

            if (resume.Resume != null)
            {
                resumeModel.WorkExperience = resume.Resume.WorkExperience;
                resumeModel.EducationInfo = resume.Resume.Education;
                resumeModel.AdditionalInfo = resume.Resume.AdditionalInformation;
                resumeModel.About = resume.Resume.AboutStudent;
                resumeModel.Dolzhnost = resume.Resume.DesiredPosition;
                resumeModel.ProffessionalSkills = resume.Resume.ProfessionalSkills;
                resumeModel.tags = resume.Resume.TagsSkills.Split(";");
            }
            else
                return RedirectToAction("Index", "ResumeStudents");

            return View("~/Views/StudentPage/StudentResume/LookResume.cshtml", resumeModel);
        }

        [HttpPost]
        public IActionResult CreateResponse(int idStudent, ResumeModel model)
        {
            ResponseFromOrganization response = new();
            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).ThenInclude(p=>p.Vacancies).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
            var student = db.Students.Include(p=>p.User).Include(p => p.Resume).Where(p => p.IdStudent == idStudent).First();
            try
            {
                response.Status = 0;
                response.DateTimeCreate = DateTime.Now;
                response.ResumeId = student.Resume.IdResume;
                response.VacancyId = organization.Vacancies.Where(p => p.IdVacancy == model.SelectedVacancy).First().IdVacancy;
                response.CommentOrganization = model.Comment?.Trim();
                db.Add(response);
                db.SaveChanges();
                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(student.User.Email, organization.Vacancies.Where(p => p.IdVacancy == model.SelectedVacancy).First().NameVacancy, organization.Vacancies.Where(p => p.IdVacancy == model.SelectedVacancy).First().Organization.NotFullNameOrganization, response.CommentOrganization, response.CommentStudent, response.Status, response.DateTimeCreate, 2);
            }
            catch { }

            return RedirectToAction("LookResume", "ResumeStudents", new { IdStudent = idStudent });
        }
    }
}

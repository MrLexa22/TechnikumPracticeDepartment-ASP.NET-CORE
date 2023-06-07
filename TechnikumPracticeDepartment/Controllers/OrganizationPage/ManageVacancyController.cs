using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsOrganizationPages;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using Org.BouncyCastle.Crypto.Agreement.JPake;

namespace TechnikumPracticeDepartment.Controllers.OrganizationPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class ManageVacancyController : Controller
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
        private IConfiguration configuration;
        public ManageVacancyController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
        }
        public IActionResult Index()
        {
            if (UpdateIn(3) == false && UpdateIn(4) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/OrganizationPage/Vacancy/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? typeSortList, string? filterTags, string search, int page = 1)
        {
            if (UpdateIn(3) == false && UpdateIn(4) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            int pageSize = 15;
            IQueryable<Vacancy> Blocks = db.Vacancies.Include(p => p.Organization).Include(p => p.ResponseFromStudents).Include(p => p.ResponseFromOrganizations).ToList().AsQueryable();
            Blocks = Blocks.Where(p => p.Organization.IsAvaliable == null || p.Organization.IsAvaliable == true);
            if (User.IsInRole("Работадатель"))
            {
                var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
                Blocks = db.Vacancies.Include(p => p.Organization).Include(p => p.ResponseFromStudents).Include(p => p.ResponseFromOrganizations).Where(p => p.OrganizationId == organization.IdOrganization).ToList().AsQueryable();
            }

            if(typeSortList == 1)
            {
                Blocks = Blocks.OrderBy(p => p.NameVacancy);
            }
            if (!String.IsNullOrEmpty(search))
            {
                Blocks = Blocks.Where(p => p.NameVacancy.ToLower().Contains(search.ToLower()));
            }
            if(filterTags != null)
            {
                Blocks = Blocks.Where(p => p.TagsSkills.ToLower().Contains(filterTags.ToLower()));
            }
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            VacancyModels model = new VacancyModels();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Vacancy(typeSortList, filterTags, search);
            model.list_vacancy = items;

            return PartialView("~/Views/OrganizationPage/Vacancy/_VacancyList.cshtml", model);
        }
        public IActionResult LookVacancy(int id)
        {
            if (UpdateIn(3) == false && UpdateIn(4) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            Organization organization = new();
            Vacancy vacancy = new();
            AddEditVacancy model = new();
            //Сотрудник организации
            if (UpdateIn(3) == true)
            {
                organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

                vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Where(p => p.IdVacancy == id).First();
                if (vacancy.OrganizationId != organization.IdOrganization)
                    return RedirectToAction("Index", "Home");
            }
            //Студент
            else if (UpdateIn(4) == true)
            {
                vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Where(p => p.IdVacancy == id).First();
                organization = db.Organizations.Where(p => p.IdOrganization == vacancy.OrganizationId).First();

                var student = db.Users.Include(p => p.Student).ThenInclude(p => p.ResponseFromStudents).
                                       Include(p => p.Student).ThenInclude(p => p.Resume).ThenInclude(p => p.ResponseFromOrganizations)
                    .Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                
                model.responseStudent = 0;
                if (student.Student.Resume == null)
                {
                    model.responseStudent = 4;
                }
                else
                {
                    if (student.Student.ResponseFromStudents.Where(p => p.VacancyId == id).Count() > 0)
                    {
                        model.responseStudent = 1;
                        model.responseId = student.Student.ResponseFromStudents.Where(p => p.VacancyId == id).First().IdResponse;
                    }
                    if (student.Student.Resume.ResponseFromOrganizations.Where(p => p.VacancyId == id).Count() > 0)
                    {
                        model.responseStudent = 2;
                        model.responseId = student.Student.Resume.ResponseFromOrganizations.Where(p => p.VacancyId == id).First().IdResponse;
                    }

                    if (student.Student.ResponseFromStudents.Count() >= 6)
                    {
                        model.responseStudent = 3;
                        model.responseId = student.Student.Resume.ResponseFromOrganizations.Where(p => p.VacancyId == id).First().IdResponse;
                    }
                }
            }
            //Администратор, сотрудник отдела пп
            else if(UpdateIn(1) == true || UpdateIn(2) == true)
            {
                vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Where(p => p.IdVacancy == id).First();
                organization = db.Organizations.Where(p => p.IdOrganization == vacancy.OrganizationId).First();
            }

            model.organization = organization;
            model.NameVacancy = vacancy.NameVacancy;
            model.IdVacancy = vacancy.IdVacancy;
            model.AdditionalInformation = vacancy.AdditionalInformation;
            model.Conditions = vacancy.Conditions;
            model.Duties = vacancy.Duties;
            model.Tags = vacancy.TagsSkills.Split(";").ToList();
            model.Description = vacancy.Description;
            model.Requirements = vacancy.Requirements;

            return View("~/Views/OrganizationPage/Vacancy/LookVacancy.cshtml", model);
        }
        public IActionResult RemoveVacancy(int id)
        {
            if (UpdateIn(3) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            Vacancy vacancy = new();
            vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).Include(p => p.ResponseFromOrganizations).Where(p => p.IdVacancy == id).First();
            if (User.IsInRole("Работадатель"))
            {
                var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
                if (vacancy.OrganizationId != organization.IdOrganization)
                    return RedirectToAction("Index", "Home");
            }
            
            db.RemoveRange(vacancy.ResponseFromOrganizations);
            db.RemoveRange(vacancy.ResponseFromStudents);
            db.Remove(vacancy);
            db.SaveChanges();

            return RedirectToAction("Index","ManageVacancy");
        }
        public async Task<IActionResult> GetListResponses(int id, int? filter, string? search, int filterList)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");

            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            var vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                ThenInclude(p => p.Student).ThenInclude(p => p.User).
                Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                Include(p=>p.ResponseFromStudents).ThenInclude(p=>p.Student).ThenInclude(p=>p.Resume).
                Include(p=>p.ResponseFromOrganizations).ThenInclude(p=>p.Resume).ThenInclude(p=>p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                Include(p => p.ResponseFromOrganizations).ThenInclude(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p=>p.User).
                Where(p => p.IdVacancy == id).First();
            if (vacancy.OrganizationId != organization.IdOrganization)
                return RedirectToAction("Index", "Home");

            AddEditVacancy model = new();
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<StudentsReposnses> listStudents = new List<StudentsReposnses>();

            if (filterList == 1)
            {
                var responses = vacancy.ResponseFromStudents.ToList();

                listStudents = responses.Select(a => new StudentsReposnses()
                {
                    student = a.Student,
                    responseFromStudent = a,
                    ID_Group = a.Student.Group.IdGroup,
                    NameGroups = a.Student.Group.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Student.Group.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation + 1)
                                                        : (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation),
                    SpecializationName = a.Student.Group.Specialization.SpecializationCode,
                    Specialization_ID = a.Student.Group.SpecializationId,
                    YearStartEducation = a.Student.Group.YearStartEducation,
                    YearOfGraduation = a.Student.Group.YearOfGraduation,
                    status = a.Status,
                    statusResponse = a.Status == 0 ? "создан" : (a.Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (a.Status == 2 ? "на рассмотрении (получен ответ студента)" : (a.Status == 3 ? "принят (на рассмотрении ОУ)" : (a.Status == 4 ? "принят" : (a.Status == 5 ? "отказ организации" : "отказ ОУ")))))
                }).ToList();
                foreach (var a in listStudents)
                {
                    if (a.IsEnded == true)
                        listStudents.Remove(a);
                }
                model.Responses_students = listStudents;
            }
            if(filterList == 2)
            {
                var responses = vacancy.ResponseFromOrganizations.ToList();

                listStudents = responses.Select(a => new StudentsReposnses()
                {
                    student = a.Resume.Student,
                    responseFromOrganization = a,
                    ID_Group = a.Resume.Student.Group.IdGroup,
                    NameGroups = a.Resume.Student.Group.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.Resume.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Resume.Student.Group.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.Resume.Student.Group.YearOfGraduation <= yearNow ? a.Resume.Student.Group.YearOfGraduation - a.Resume.Student.Group.YearStartEducation : yearNow - a.Resume.Student.Group.YearStartEducation + 1)
                                                        : (a.Resume.Student.Group.YearOfGraduation <= yearNow ? a.Resume.Student.Group.YearOfGraduation - a.Resume.Student.Group.YearStartEducation : yearNow - a.Resume.Student.Group.YearStartEducation),
                    SpecializationName = a.Resume.Student.Group.Specialization.SpecializationCode,
                    Specialization_ID = a.Resume.Student.Group.SpecializationId,
                    YearStartEducation = a.Resume.Student.Group.YearStartEducation,
                    YearOfGraduation = a.Resume.Student.Group.YearOfGraduation,
                    status = a.Status,
                    statusResponse = a.Status == 0 ? "создан" : (a.Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (a.Status == 2 ? "на рассмотрении (получен ответ студента)" : (a.Status == 3 ? "принят (на рассмотрении ОУ)" : (a.Status == 4 ? "принят" : (a.Status == 5 ? "отказ студента" : "отказ ОУ")))))
                }).ToList();
                foreach (var a in listStudents)
                {
                    if (a.IsEnded == true)
                        listStudents.Remove(a);
                }
                model.Responses_students = listStudents;
            }

            if (filter > -1)
                model.Responses_students = model.Responses_students.Where(p => p.status == filter).ToList();
            if (!String.IsNullOrEmpty(search))
                model.Responses_students = model.Responses_students.Where(p => p.student.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.student.User.NameUser.ToLower().Contains(search.ToLower())).ToList();
            model.IdVacancy = vacancy.IdVacancy;
            if (filterList == 1)
                model.Responses_students = model.Responses_students.OrderBy(p => p.responseFromStudent.DateTimeCreate).ToList();
            if (filterList == 2)
                model.Responses_students = model.Responses_students.OrderBy(p => p.responseFromOrganization.DateTimeCreate).ToList();
            return PartialView("~/Views/OrganizationPage/Vacancy/_ListResponsesStudents.cshtml", model);
        }
        public async Task<IActionResult> InfoResponse(int IdStudent, int IdVacancy, int typeResponse, int fromDelete)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");

            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            var vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                ThenInclude(p => p.Student).ThenInclude(p => p.User).
                Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Resume).
                Include(p=>p.ResponseFromOrganizations).ThenInclude(p=>p.Resume).ThenInclude(p=>p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                Include(p => p.ResponseFromOrganizations).ThenInclude(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p=>p.User).
                Where(p => p.IdVacancy == IdVacancy).First();

            if (vacancy.OrganizationId != organization.IdOrganization)
                return RedirectToAction("Index", "Home");

            ConcatinationStudentWithPractice model = new();
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<StudentsReposnses> listStudents = new List<StudentsReposnses>();

            if (typeResponse == 1)
            {
                var responses = vacancy.ResponseFromStudents.Where(p => p.StudentId == IdStudent).ToList();
                listStudents = responses.Select(a => new StudentsReposnses()
                {
                    student = a.Student,
                    responseFromStudent = a,
                    ID_Group = a.Student.Group.IdGroup,
                    NameGroups = a.Student.Group.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Student.Group.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation + 1)
                                                        : (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation),
                    SpecializationName = a.Student.Group.Specialization.SpecializationCode,
                    Specialization_ID = a.Student.Group.SpecializationId,
                    YearStartEducation = a.Student.Group.YearStartEducation,
                    YearOfGraduation = a.Student.Group.YearOfGraduation,
                    status = a.Status,
                    statusResponse = a.Status == 0 ? "создан" : (a.Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (a.Status == 2 ? "на рассмотрении (получен ответ студента)" : (a.Status == 3 ? "принят (на рассмотрении ОУ)" : (a.Status == 4 ? "принят" : (a.Status == 5 ? "отказ организации" : "отказ ОУ")))))
                }).ToList();
            }
            if(typeResponse == 2) 
            {
                var responses = vacancy.ResponseFromOrganizations.Where(p => p.Resume.StudentId == IdStudent).ToList();
                listStudents = responses.Select(a => new StudentsReposnses()
                {
                    student = a.Resume.Student,
                    responseFromOrganization = a,
                    ID_Group = a.Resume.Student.Group.IdGroup,
                    NameGroups = a.Resume.Student.Group.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.Resume.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Resume.Student.Group.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.Resume.Student.Group.YearOfGraduation <= yearNow ? a.Resume.Student.Group.YearOfGraduation - a.Resume.Student.Group.YearStartEducation : yearNow - a.Resume.Student.Group.YearStartEducation + 1)
                                                        : (a.Resume.Student.Group.YearOfGraduation <= yearNow ? a.Resume.Student.Group.YearOfGraduation - a.Resume.Student.Group.YearStartEducation : yearNow - a.Resume.Student.Group.YearStartEducation),
                    SpecializationName = a.Resume.Student.Group.Specialization.SpecializationCode,
                    Specialization_ID = a.Resume.Student.Group.SpecializationId,
                    YearStartEducation = a.Resume.Student.Group.YearStartEducation,
                    YearOfGraduation = a.Resume.Student.Group.YearOfGraduation,
                    status = a.Status,
                    statusResponse = a.Status == 0 ? "создан" : (a.Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (a.Status == 2 ? "на рассмотрении (получен ответ студента)" : (a.Status == 3 ? "принят (на рассмотрении ОУ)" : (a.Status == 4 ? "принят" : (a.Status == 5 ? "отказ организации" : "отказ ОУ")))))
                }).ToList();
            }

            var studentInfo = listStudents.First();
            model.response = new();
            model.students = studentInfo;
            model.vacancy = vacancy;
            //В вакансии = 1; На странице откликов = 2. На странице резюме = 3
            model.fromDeletet = fromDelete;
            model.typeResponse = 1;
            if (typeResponse == 1)
                model.response = studentInfo.responseFromStudent;
            else
            {
                model.response2 = studentInfo.responseFromOrganization;

                model.response.CommentStudent = studentInfo.responseFromOrganization.CommentStudent;
                model.response.CommentOrganization = studentInfo.responseFromOrganization.CommentOrganization;
                model.response.DateTimeCreate = studentInfo.responseFromOrganization.DateTimeCreate;
                model.response.Vacancy = studentInfo.responseFromOrganization.Vacancy;
                model.response.Status = studentInfo.responseFromOrganization.Status;
                model.response.Student = studentInfo.responseFromOrganization.Resume.Student;
                model.response.IdResponse = studentInfo.responseFromOrganization.IdResponse;
                model.response.VacancyId = studentInfo.responseFromOrganization.VacancyId;
                model.typeResponse = 2;
            }
            
            return PartialView("~/Views/OrganizationPage/Vacancy/_InfoResponseFromStudent.cshtml", model);
        }
        public async Task<IActionResult> EditStatus(ConcatinationStudentWithPractice model, int id, int typeResponse)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");

            if (typeResponse == 1)
            {
                var response = db.ResponseFromStudents.Include(p => p.Student).ThenInclude(p => p.User).Where(p => p.IdResponse == id).First();

                var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

                var vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Resume).
                    Include(p => p.Organization).
                    Where(p => p.IdVacancy == response.VacancyId).First();

                if (vacancy.OrganizationId != organization.IdOrganization)
                    return RedirectToAction("Index", "Home");

                if (response.Status == 0 || response.Status == 1 || response.Status == 2 || response.Status == 5)
                    response.Status = model.response.Status;

                response.CommentOrganization = model.response.CommentOrganization?.Trim();
                db.Update(response);
                db.SaveChanges();
                string email_student = response.Student.User.Email;

                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(email_student, vacancy.NameVacancy, vacancy.Organization.NotFullNameOrganization, response.CommentOrganization, response.CommentStudent, response.Status, response.DateTimeCreate, 1);
            }
            if(typeResponse == 2)
            {
                var response = db.ResponseFromOrganizations.Include(p=>p.Vacancy).Include(p=>p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User).Where(p => p.IdResponse == id).First();
                var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
                if(response.Vacancy.OrganizationId != organization.IdOrganization)
                    return RedirectToAction("Index", "Home");

                var vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Resume).
                    Include(p => p.Organization).
                    Where(p => p.IdVacancy == response.VacancyId).First();

                response.Status = 1;
                response.CommentOrganization = model.response.CommentOrganization?.Trim();
                db.Update(response);
                db.SaveChanges();
                string email_student = response.Resume.Student.User.Email;

                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(email_student, vacancy.NameVacancy, vacancy.Organization.NotFullNameOrganization, response.CommentOrganization, response.CommentStudent, response.Status, response.DateTimeCreate, 1);
            }

            return NoContent();
        }
        public bool checkNameVacancy(string NameVacancy, int IdVacancy, int organizationID)
        {
            if (NameVacancy.Trim().Length < 4 || NameVacancy.Trim().Length > 50)
                return false;
            if (NameVacancy.Contains("\n"))
                return false;
           if(db.Vacancies.Where(p=>p.OrganizationId == organizationID && p.NameVacancy.ToLower() == NameVacancy.Trim().ToLower() && p.IdVacancy != IdVacancy).Count() > 0)
                return false;
            return true;
        }
        public bool checkDescription(string Description)
        {
            int howMuchN = Description.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < Description.Length; i++)
                {
                    try
                    {
                        if (Description[i] == '\n' && Description[i + 1] == '\n' && Description[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (Description[Description.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (Description.Trim().Length < 10 || Description.Trim().Length > 2000)
                return false;
            return true;
        }
        public bool checkDuties(string Duties)
        {
            int howMuchN = Duties.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < Duties.Length; i++)
                {
                    try
                    {
                        if (Duties[i] == '\n' && Duties[i + 1] == '\n' && Duties[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (Duties[Duties.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (Duties.Trim().Length < 10 || Duties.Trim().Length > 2000)
                return false;
            return true;
        }
        public bool checkRequirements(string Requirements)
        {
            int howMuchN = Requirements.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < Requirements.Length; i++)
                {
                    try
                    {
                        if (Requirements[i] == '\n' && Requirements[i + 1] == '\n' && Requirements[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (Requirements[Requirements.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (Requirements.Trim().Length < 10 || Requirements.Trim().Length > 2000)
                return false;
            return true;
        }
        public bool checkConditions(string Conditions)
        {
            int howMuchN = Conditions.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < Conditions.Length; i++)
                {
                    try
                    {
                        if (Conditions[i] == '\n' && Conditions[i + 1] == '\n' && Conditions[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (Conditions[Conditions.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (Conditions.Trim().Length < 10 || Conditions.Trim().Length > 2000)
                return false;
            return true;
        }
        public bool checkAdditionalInformation(string AdditionalInformation)
        {
            int howMuchN = AdditionalInformation.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < AdditionalInformation.Length; i++)
                {
                    try
                    {
                        if (AdditionalInformation[i] == '\n' && AdditionalInformation[i + 1] == '\n' && AdditionalInformation[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (AdditionalInformation[AdditionalInformation.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (AdditionalInformation.Trim().Length < 10 || AdditionalInformation.Trim().Length > 2000)
                return false;
            return true;
        }
        public IActionResult AddEditVacancy(int id)
        {
            if (UpdateIn(3) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            if(id == 0 && (User.IsInRole("Администратор") || User.IsInRole("Сотрудник производственного отдела")))
                return RedirectToAction("Index", "Home");
            
            AddEditVacancy model = new();

            Organization organization = new();
            if (User.IsInRole("Работадатель"))
            {
                organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
                model.organization = organization;
                model.organizationID = organization.IdOrganization;
            }

            if (id != 0)
            {
                var vacancy = db.Vacancies.Include(p=>p.Organization).Include(p => p.ResponseFromStudents).
                    ThenInclude(p => p.Student).ThenInclude(p => p.User).
                    Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                    Where(p => p.IdVacancy == id).First();

                if (User.IsInRole("Работадатель"))
                {
                    if (vacancy.OrganizationId != organization.IdOrganization)
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    model.organization = vacancy.Organization;
                    model.organizationID = vacancy.OrganizationId;
                }

                model.NameVacancy = vacancy.NameVacancy;
                model.IdVacancy = vacancy.IdVacancy;
                model.AdditionalInformation = vacancy.AdditionalInformation;
                model.Conditions = vacancy.Conditions;
                model.Duties = vacancy.Duties;
                model._tags = vacancy.TagsSkills.Split(";").ToArray();
                model.Description = vacancy.Description;
                model.Requirements = vacancy.Requirements;
            }

            return View("~/Views/OrganizationPage/Vacancy/AddEditVacancy.cshtml", model);
        }
        
        [HttpPost]        
        public IActionResult AddEditVacancyPost(int id, AddEditVacancy model)
        {
            if (UpdateIn(3) == false && UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (id == 0 && (User.IsInRole("Администратор") || User.IsInRole("Сотрудник производственного отдела")))
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                Organization organization = new();
                Vacancy vacancy = new();

                if (User.IsInRole("Работадатель"))
                {
                    organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;
                    if (id > 0)
                    {
                        var vacancyCheck = db.Vacancies.Include(p => p.ResponseFromStudents).
                            ThenInclude(p => p.Student).ThenInclude(p => p.User).
                            Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                            Where(p => p.IdVacancy == id).First();
                        if (vacancyCheck.OrganizationId != organization.IdOrganization)
                            return RedirectToAction("Index", "Home");
                        else
                            vacancy = db.Vacancies.Where(p => p.IdVacancy == id).First();
                    }
                }
                else
                {
                    vacancy = db.Vacancies.Include(p=>p.Organization).Where(p => p.IdVacancy == id).First();
                    organization = vacancy.Organization;
                }
                vacancy.NameVacancy = model.NameVacancy.Trim();
                vacancy.Organization = organization;
                vacancy.AdditionalInformation = model.AdditionalInformation.Trim();
                vacancy.Conditions = model.Conditions.Trim();
                vacancy.Duties = model.Duties.Trim();
                vacancy.Description = model.Description.Trim();
                vacancy.TagsSkills = string.Join(";", model._tags);
                vacancy.Requirements = model.Requirements.Trim();
                if (id == 0)
                    db.Vacancies.Add(vacancy);
                else
                    db.Vacancies.Update(vacancy);
                db.SaveChanges();
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = vacancy.IdVacancy });
            }

            return RedirectToAction("AddEditVacancy", "ManageVacancy", new {id = id});
        }
        public async Task<IActionResult> DeleteResponse(int idResponse, int fromDelete)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");

            var response = db.ResponseFromOrganizations.Include(p=>p.Vacancy).Include(p=>p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User).Where(p => p.IdResponse == idResponse).First();
            int idVacancy = response.VacancyId;
            int idResume = response.Resume.StudentId;
            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            if (response.Vacancy.OrganizationId != organization.IdOrganization)
                return RedirectToAction("Index", "Home");

            db.ResponseFromOrganizations.Remove(response);
            db.SaveChanges();
            if (fromDelete == 1)
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = idVacancy });
            else if(fromDelete == 2)
                return RedirectToAction("Index", "ManageResponsesOrganization");
            else if(fromDelete == 3)
                return RedirectToAction("LookResume", "ResumeStudents", new { IdStudent = idResume });

            return NoContent();
        }

        //Студент
        public IActionResult CreateResponse(int idVacancy)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            var student = db.Users.Include(p => p.Student).ThenInclude(p=>p.Resume).Include(p => p.Student).ThenInclude(p=>p.ResponseFromStudents).ThenInclude(p=>p.Vacancy).ThenInclude(p=>p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            if(student.Student.Resume == null)
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = idVacancy });

            if (student.Student.ResponseFromStudents.Where(p=>p.Vacancy.Organization.IsAvaliable != false).Count() >= 6)
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = idVacancy });

            if(student.Student.ResponseFromStudents.Where(p=>p.VacancyId == idVacancy).Count() > 0)
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = idVacancy });

            ResponseFromStudent response = new();
            response.Status = 0;
            response.StudentId = student.Student.IdStudent;
            response.DateTimeCreate = DateTime.Now;
            response.VacancyId = idVacancy;
            db.ResponseFromStudents.Add(response);
            db.SaveChanges();

            return RedirectToAction("LookVacancy","ManageVacancy", new {id = idVacancy });
        }
    }
}

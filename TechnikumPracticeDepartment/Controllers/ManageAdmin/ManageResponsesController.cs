using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsManageResponses;
using DocumentFormat.OpenXml.Spreadsheet;
using TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using Org.BouncyCastle.Asn1.Ocsp;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Excel;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class ManageResponsesController : Controller
    {
        public static bool IsTimeInGivenPeriods(DateTime timeToCheck, IEnumerable<Periods> periods)
        {
            bool result = periods?.Any(p => (timeToCheck >= p.Start || timeToCheck <= p.Start) && timeToCheck <= p.End) ?? false;
            return result;
        }
        public static bool CheckIsTimeInGivenPeriods(DateTime timeToCheck, IEnumerable<Periods> periods)
        {
            bool result = periods?.Any(p => timeToCheck >= p.Start && timeToCheck <= p.End) ?? false;
            return result;
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

        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public ManageResponsesController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
        }
        public IActionResult Index()
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/ManageResponses/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? sortList, int? filterListType, int? filterListStatus, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            IndexManageResponses model = new IndexManageResponses();
            model.list_Responses = new();
            model.list_Requests = new();
            int pageSize = 15;
            if ((filterListType == null || filterListType == 0 || filterListType == 2) && (filterListStatus != 0 && filterListStatus != 1))
            {
                var ResponsesFromSt = db.ResponseFromStudents.Include(p=>p.Student).ThenInclude(p=>p.Resume).Include(p => p.Vacancy).ThenInclude(p => p.Organization).Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p=>p.Specialization).Where(p => p.Status == 3 || p.Status == 4 || p.Status == 6).ToList();
                var ResponsesFromOrg = db.ResponseFromOrganizations.Include(p => p.Vacancy).ThenInclude(p => p.Organization).Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User).Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p=>p.Specialization).Where(p => p.Status == 3 || p.Status == 4 || p.Status == 6).ToList();
                if(filterListStatus == null || filterListStatus == -1 || filterListStatus == 3)
                {
                    ResponsesFromSt = ResponsesFromSt.Where(p => p.Status == 3).ToList();
                    ResponsesFromOrg = ResponsesFromOrg.Where(p => p.Status == 3).ToList();
                }
                if(filterListStatus == 4 || filterListStatus == 6)
                {
                    ResponsesFromSt = ResponsesFromSt.Where(p => p.Status == filterListStatus).ToList();
                    ResponsesFromOrg = ResponsesFromOrg.Where(p => p.Status == filterListStatus).ToList();
                }
                foreach (var a in ResponsesFromSt)
                {
                    ResponseFromOrganizationOrStudent res = new();
                    res.ResponseFromStudent = a;
                    res.ResponseFromOrganization = null;
                    model.list_Responses.Add(res);
                }
                foreach (var a in ResponsesFromOrg)
                {
                    ResponseFromOrganizationOrStudent res = new();
                    res.ResponseFromStudent = null;
                    res.ResponseFromOrganization = a;
                    model.list_Responses.Add(res);
                }
            }

            if ((filterListType == null || filterListType == 0 || filterListType == 1) && filterListStatus != 3)
            {
                var RequestsToDistr = db.RequestToDistributuions.Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p=>p.Specialization).ToList();
                model.list_Requests = RequestsToDistr;
                if (filterListStatus == null || filterListStatus == -1)
                    model.list_Requests = model.list_Requests.Where(p => p.StatusReuqest == 0 || p.StatusReuqest == 1).ToList();
                if(filterListStatus > -1 && filterListStatus < 2)
                    model.list_Requests = model.list_Requests.Where(p => p.StatusReuqest == filterListStatus).ToList();
                if(filterListStatus == 6)
                    model.list_Requests = model.list_Requests.Where(p => p.StatusReuqest == 3).ToList();
            }

            if(sortList == 1)
            {
                model.list_Requests = model.list_Requests.OrderBy(p=>p.IdRequest).ToList();
                model.list_Responses = model.list_Responses.OrderBy(p => p.ResponseFromStudent?.DateTimeCreate).OrderBy(p=>p.ResponseFromOrganization?.DateTimeCreate).ToList();
            }
            if (sortList == 2)
            {
                model.list_Requests = model.list_Requests.OrderByDescending(p => p.IdRequest).ToList();
                model.list_Responses = model.list_Responses.OrderByDescending(p => p.ResponseFromStudent?.DateTimeCreate).OrderByDescending(p => p.ResponseFromOrganization?.DateTimeCreate).ToList();
            }
            if (!String.IsNullOrEmpty(search))
            {
                model.list_Requests = model.list_Requests.Where(p => p.Student.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.Student.User.NameUser.ToLower().Contains(search.ToLower())).ToList();
                model.list_Responses = model.list_Responses.Where(p => p.ResponseFromStudent != null ? (p.ResponseFromStudent.Student.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.ResponseFromStudent.Student.User.NameUser.ToLower().Contains(search.ToLower()) || p.ResponseFromStudent.Student.Resume.DesiredPosition.ToLower().Contains(search.ToLower()) || p.ResponseFromStudent.Vacancy.NameVacancy.ToLower().Contains(search.ToLower())) : (p.ResponseFromOrganization.Resume.Student.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.ResponseFromOrganization.Resume.Student.User.NameUser.ToLower().Contains(search.ToLower()) || p.ResponseFromOrganization.Resume.DesiredPosition.ToLower().Contains(search.ToLower()) || p.ResponseFromOrganization.Vacancy.NameVacancy.ToLower().Contains(search.ToLower()))).ToList();
            }

            IQueryable<ResponseFromOrganizationOrStudent> Blocks = model.list_Responses.AsQueryable();
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            IQueryable<RequestToDistributuion> Blocks1 = model.list_Requests.AsQueryable();
            var items1 = Blocks1.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            
            int count = 0;
            if(filterListType == 1)
                count = Blocks1.Count();
            if(filterListType == 2)
                count = Blocks.Count();
            if(filterListType == null || filterListType == 0)
                count = (Blocks1.Count() + Blocks.Count())/2;

            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_ManageResponses(sortList, filterListType, filterListStatus, search);
            model.list_Responses = items;
            model.list_Requests = items1;

            return PartialView("~/Views/ManageAdmin/ManageResponses/_ListResponses.cshtml", model);
        }
        
        //---Запросы на распределение по договору---//
        public IActionResult InfoRequest(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            ResponseDistributionFromStudent model = new();
            RequestToDistributuion request = db.RequestToDistributuions.Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).Where(p => p.IdRequest == id).FirstOrDefault();
            model.InnOrganization = request.InnOrganization;
            model.IdRequest = request.IdRequest;
            model.StudentId = request.StudentId;
            model.request = request;
            model.AddressOrganization = request.AddressOrganization;
            model.FullNameOrganization = request.FullNameOrganization;
            model.NotFullNameOrganization = request.NotFullNameOrganization;
            model.SurnameContactNameOrganization = request.SurnameContactNameOrganization;
            model.NameContactNameOrganization = request.NameContactNameOrganization;
            model.PatronymicNameContactNameOrganization = request.PatronymicNameContactNameOrganization;
            model.Email = request.EmailContactNameOrganization;
            model.PhoneNumber = request.PhoneNumberContactNameOrganization;
            model.StatusReuqest = request.StatusReuqest;
            model.EmployeeOfTechnikum = request.EmployeeOfTechnikum;
            var organization = db.Organizations.Where(p => p.InnOrganization == request.InnOrganization || p.FullNameOrganization.ToLower() == request.FullNameOrganization.ToLower() || p.NotFullNameOrganization.ToLower() == request.NotFullNameOrganization.ToLower()).ToList();
            if(organization.Count() <= 0)
            {
                model.isExist = false;
                model.existOrganization = null;
            }
            else
            {
                model.isExist = true;
                model.existOrganization = organization.First();
            }

            var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                Where(p => p.IdGroup == request.Student.GroupId).FirstOrDefault();

            var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
            list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();
            model.list_practice = new List<Practice_DistributionStudent>();
            foreach (var o in list_practices)
            {
                o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == request.Student.GroupId).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                if (o.PracticeCharts.Count() > 0)
                {
                    Practice_DistributionStudent practic = new Practice_DistributionStudent();
                    practic.NamePractice = o.NamePractice;
                    practic.ID_Practice = o.IdPractice;
                    var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                    practic.list_dates = new List<PracticesChartDate>();

                    foreach (var dath in o.PracticeCharts)
                    {
                        foreach (var dath2 in dath.PracticesChartDates)
                        {
                            practic.list_dates.Add(dath2);
                        }
                    }

                    TimeOnly time = new TimeOnly(0, 0, 0);
                    if (IsTimeInGivenPeriods(DateTime.Now, practic.list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                    {
                        practic.IsEnded = true;
                    }
                    else
                    {
                        practic.IsEnded = false;
                    }

                    practic.list_distributesStudents = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).Where(p => p.Student.Group.IdGroup == request.Student.GroupId).ToList();
                    model.list_practice.Add(practic);
                }
            }
            model.list_practice = model.list_practice.Where(p=>p.IsEnded == false).OrderBy(p => p.list_dates.Select(p => p.DateEnd).First()).ToList();
            return View("~/Views/ManageAdmin/ManageResponses/RequestInfo.cshtml", model);
        }
        public IActionResult ChangeStatusRequest(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            RequestToDistributuion request = db.RequestToDistributuions.Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).Where(p => p.IdRequest == id).FirstOrDefault();
            var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                Where(p => p.IdGroup == request.Student.GroupId).FirstOrDefault();

            var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
            list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();
            var list_practice = new List<Practice_DistributionStudent>();
            foreach (var o in list_practices)
            {
                o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == request.Student.GroupId).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                if (o.PracticeCharts.Count() > 0)
                {
                    Practice_DistributionStudent practic = new Practice_DistributionStudent();
                    practic.NamePractice = o.NamePractice;
                    practic.ID_Practice = o.IdPractice;
                    var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                    practic.list_dates = new List<PracticesChartDate>();

                    foreach (var dath in o.PracticeCharts)
                    {
                        foreach (var dath2 in dath.PracticesChartDates)
                        {
                            practic.list_dates.Add(dath2);
                        }
                    }

                    TimeOnly time = new TimeOnly(0, 0, 0);
                    if (IsTimeInGivenPeriods(DateTime.Now, practic.list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                    {
                        practic.IsEnded = true;
                    }
                    else
                    {
                        practic.IsEnded = false;
                    }

                    practic.list_distributesStudents = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).Where(p => p.Student.Group.IdGroup == request.Student.GroupId).ToList();
                    list_practice.Add(practic);
                }
            }
            list_practice = list_practice.Where(p => p.IsEnded == false).OrderBy(p => p.list_dates.Select(p => p.DateEnd).First()).ToList();

            if(list_practice.Count() > 0)
            {
                request.StatusReuqest = 1;
                request.EmployeeOfTechnikum = userDB;

                _ = new EmailService(configuration).SendEmailWithStatusRequest(request.Student.User.Email, "Ожидание подписанных экземпляров договоров организацией");
                db.Update(request);
                db.SaveChanges();
            }

            return RedirectToAction("InfoRequest", "ManageResponses", new { id = id });
        }
        public IActionResult DiscardRequest(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            RequestToDistributuion request = db.RequestToDistributuions.Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).Where(p => p.IdRequest == id).FirstOrDefault();
            var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            request.StatusReuqest = 3;
            request.EmployeeOfTechnikum = userDB;
            _ = new EmailService(configuration).SendEmailWithStatusRequest(request.Student.User.Email, "Заявка отклонена. Обратитесь в отдел производственного обучения");
            db.Update(request);
            db.SaveChanges();

            return RedirectToAction("InfoRequest", "ManageResponses", new { id = id });
        }
        public IActionResult ChangeStatusRequestDistribution(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            RequestToDistributuion request = db.RequestToDistributuions.Include(p=>p.Student).ThenInclude(p=>p.Resume).Include(p => p.Student).ThenInclude(p => p.User).Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).Where(p => p.IdRequest == id).FirstOrDefault();
            var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                Where(p => p.IdGroup == request.Student.GroupId).FirstOrDefault();

            var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
            list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();
            var list_practice = new List<Practice_DistributionStudent>();
            foreach (var o in list_practices)
            {
                o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == request.Student.GroupId).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                if (o.PracticeCharts.Count() > 0)
                {
                    Practice_DistributionStudent practic = new Practice_DistributionStudent();
                    practic.NamePractice = o.NamePractice;
                    practic.ID_Practice = o.IdPractice;
                    var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                    practic.list_dates = new List<PracticesChartDate>();

                    foreach (var dath in o.PracticeCharts)
                    {
                        foreach (var dath2 in dath.PracticesChartDates)
                        {
                            practic.list_dates.Add(dath2);
                        }
                    }

                    TimeOnly time = new TimeOnly(0, 0, 0);
                    if (IsTimeInGivenPeriods(DateTime.Now, practic.list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                    {
                        practic.IsEnded = true;
                    }
                    else
                    {
                        practic.IsEnded = false;
                    }

                    practic.list_distributesStudents = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).Where(p => p.Student.Group.IdGroup == request.Student.GroupId).ToList();
                    list_practice.Add(practic);
                }
            }
            list_practice = list_practice.Where(p => p.IsEnded == false).OrderBy(p => p.list_dates.Select(p => p.DateEnd).First()).ToList();

            if (list_practice.Count() > 0)
            {
                request.StatusReuqest = 2;
                request.EmployeeOfTechnikum = userDB;
                _ = new EmailService(configuration).SendEmailWithStatusRequest(request.Student.User.Email, "Заявка завершена. Вы распределены в указанную организацию. Подробнее смотрите на сайте");
                db.Update(request);

                int IdOrganization = 0;
                var organization = db.Organizations.Where(p => p.InnOrganization == request.InnOrganization || p.FullNameOrganization.ToLower() == request.FullNameOrganization.ToLower() || p.NotFullNameOrganization.ToLower() == request.NotFullNameOrganization.ToLower()).ToList();
                if (organization.Count() <= 0)
                {
                    IdOrganization = 0;
                }
                else
                {
                    IdOrganization = organization.First().IdOrganization;
                }
                if (IdOrganization == 0)
                {
                    Organization organ = new();
                    organ.InnOrganization = request.InnOrganization;
                    organ.FullNameOrganization = request.FullNameOrganization;
                    organ.NotFullNameOrganization = request.NotFullNameOrganization;
                    organ.SurnameContactOfOrganization = request.SurnameContactNameOrganization;
                    organ.NameContactOfOrganization = request.NameContactNameOrganization;
                    organ.PatronymicContactOfOrganization = request.PatronymicNameContactNameOrganization;
                    organ.PhoneNumberContactOfOrganization = request.PhoneNumberContactNameOrganization;
                    organ.AddressOrganization = request.AddressOrganization;
                    db.Add(organ);
                    db.SaveChanges();
                    IdOrganization = organ.IdOrganization;
                }
                foreach (var a in list_practice)
                {
                    if (db.PracticeChartDistibutions.Where(p => p.PracticeId == a.ID_Practice && p.StudentId == request.StudentId).Count() <= 0)
                    {
                        PracticeChartDistibution practice = new();
                        practice.PracticeId = a.ID_Practice;
                        practice.OrganizationId = IdOrganization;
                        practice.StudentId = request.StudentId;
                        db.Add(practice);
                    }
                    else
                    {
                        PracticeChartDistibution practice = db.PracticeChartDistibutions.Where(p => p.PracticeId == a.ID_Practice && p.StudentId == request.StudentId).First();
                        practice.OrganizationId = IdOrganization;
                        db.Update(practice);
                    }
                }
                if(request.Student.Resume != null)
                {
                    request.Student.Resume.IsAvaliable = false;
                    db.Resumes.Update(request.Student.Resume);
                }
                db.SaveChanges();
            }

            return RedirectToAction("InfoRequest", "ManageResponses", new { id = id });
        }
        
        //--Отклики--//
        public IActionResult InfoResponse(int id, int typeResponse, bool? error, string? errorText)
        {
            ResponseFromOrganizationOrStudent model = new();
            if (error == null || error == false)
                model.error = false;
            else
            {
                model.error = true;
                model.ErrorText = errorText;
            }

            int IdGroup = 0;
            model.typeResponse = typeResponse;

            if (typeResponse == 1)
            {
                ResponseFromStudent responseFromStudent = new();
                responseFromStudent = db.ResponseFromStudents
                                        .Include(p=>p.Student).ThenInclude(p=>p.Group).ThenInclude(p=>p.Specialization)
                                        .Include(p=>p.Student).ThenInclude(p=>p.User)
                                        .Include(p => p.Student).ThenInclude(p => p.Resume)
                                        .Include(p=>p.Vacancy).ThenInclude(p=>p.Organization)
                                        .Where(p=>p.IdResponse == id).First();
                IdGroup = responseFromStudent.Student.GroupId;
                model.ResponseFromStudent = responseFromStudent;
                model.StatusResponse = responseFromStudent.Status;
            }
            if(typeResponse == 2)
            {
                ResponseFromOrganization responseFromOrganization = new();
                responseFromOrganization = db.ResponseFromOrganizations
                        .Include(p=>p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization)
                        .Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User)
                        .Include(p => p.Vacancy).ThenInclude(p => p.Organization)
                        .Where(p => p.IdResponse == id).First();
                IdGroup = responseFromOrganization.Resume.Student.GroupId;
                model.ResponseFromOrganization = responseFromOrganization;
                model.StatusResponse = responseFromOrganization.Status;
            }

            var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                Where(p => p.IdGroup == IdGroup).FirstOrDefault();

            var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
            list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();
            model.list_practice = new List<Practice_DistributionStudent_Boolean>();
            foreach (var o in list_practices)
            {
                o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == IdGroup).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                if (o.PracticeCharts.Count() > 0)
                {
                    Practice_DistributionStudent_Boolean practic = new Practice_DistributionStudent_Boolean();
                    practic.NamePractice = o.NamePractice;
                    practic.ID_Practice = o.IdPractice;
                    var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                    practic.list_dates = new List<PracticesChartDate>();

                    foreach (var dath in o.PracticeCharts)
                    {
                        foreach (var dath2 in dath.PracticesChartDates)
                        {
                            practic.list_dates.Add(dath2);
                        }
                    }

                    TimeOnly time = new TimeOnly(0, 0, 0);
                    if (IsTimeInGivenPeriods(DateTime.Now, practic.list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                    {
                        practic.IsEnded = true;
                    }
                    else
                    {
                        practic.IsEnded = false;
                    }

                    practic.list_distributesStudents = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).Where(p => p.Student.Group.IdGroup == IdGroup).ToList();
                    model.list_practice.Add(practic);
                }
            }
            model.list_practice = model.list_practice.Where(p => p.IsEnded == false).OrderBy(p => p.list_dates.Select(p => p.DateEnd).First()).ToList();
            return View("~/Views/ManageAdmin/ManageResponses/ResponseInfo.cshtml", model);
        }

        [HttpPost]
        public IActionResult DiscardResponse(int id, int typeResponse, ResponseFromOrganizationOrStudent model)
        {
            int IdGroup = 0;
            int status = 0;
            if(typeResponse == 1)
            {
                ResponseFromStudent responseFromStudent = new();
                responseFromStudent = db.ResponseFromStudents
                                        .Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization)
                                        .Include(p => p.Student).ThenInclude(p => p.User)
                                        .Include(p => p.Student).ThenInclude(p => p.Resume)
                                        .Include(p => p.Vacancy).ThenInclude(p => p.Organization)
                                        .Where(p => p.IdResponse == id).First();
                IdGroup = responseFromStudent.Student.GroupId;
                status = responseFromStudent.Status;

                if (status != 3)
                    return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse, error = true, errorText = "Непредвиденная ошибка" });

                if (responseFromStudent.CommentOrganization == null && model.CommentDiscard != null && model.CommentDiscard?.Trim().Length > 0)
                    responseFromStudent.CommentOrganization = "Комментарий отдела производтсвенного обучения:\n" + model.CommentDiscard.Trim();
                if (responseFromStudent.CommentOrganization != null && model.CommentDiscard != null && model.CommentDiscard?.Trim().Length > 0)
                    responseFromStudent.CommentOrganization += "\n\nКомментарий отдела производтсвенного обучения:\n" + model.CommentDiscard.Trim();

                responseFromStudent.Status = 6;
                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(responseFromStudent.Student.User.Email, responseFromStudent.Vacancy.NameVacancy, responseFromStudent.Vacancy.Organization.NotFullNameOrganization, responseFromStudent.CommentOrganization, responseFromStudent.CommentStudent, responseFromStudent.Status, responseFromStudent.DateTimeCreate, 1);
                db.ResponseFromStudents.Update(responseFromStudent);
                db.SaveChanges();
            }
            if (typeResponse == 2)
            {
                ResponseFromOrganization responseFromOrganization = new();
                responseFromOrganization = db.ResponseFromOrganizations
                        .Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization)
                        .Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User)
                        .Include(p => p.Vacancy).ThenInclude(p => p.Organization)
                        .Where(p => p.IdResponse == id).First();

                IdGroup = responseFromOrganization.Resume.Student.GroupId;
                status = responseFromOrganization.Status;

                if (status != 3)
                    return RedirectToAction("InfoResponse", "ManageResponses", new {id=id, typeResponse = typeResponse, error = true, errorText = "Непредвиденная ошибка"});

                if (responseFromOrganization.CommentOrganization == null && model.CommentDiscard != null && model.CommentDiscard?.Trim().Length > 0)
                    responseFromOrganization.CommentOrganization = "Комментарий отдела производтсвенного обучения:\n"+model.CommentDiscard.Trim();
                if(responseFromOrganization.CommentOrganization != null && model.CommentDiscard != null && model.CommentDiscard?.Trim().Length > 0)
                    responseFromOrganization.CommentOrganization += "\n\nКомментарий отдела производтсвенного обучения:\n" + model.CommentDiscard.Trim();

                responseFromOrganization.Status = 6;
                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(responseFromOrganization.Resume.Student.User.Email, responseFromOrganization.Vacancy.NameVacancy, responseFromOrganization.Vacancy.Organization.NotFullNameOrganization, responseFromOrganization.CommentOrganization, responseFromOrganization.CommentStudent, responseFromOrganization.Status, responseFromOrganization.DateTimeCreate, 1);
                db.ResponseFromOrganizations.Update(responseFromOrganization);
                db.SaveChanges();
            }
            return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse });
        }

        [HttpPost]
        public IActionResult AcceptResponse(int id, int typeResponse, ResponseFromOrganizationOrStudent model)
        {
            int IdGroup = 0;
            int status = 0;
            int IdOrganization = 0;
            int IdStudent = 0;

            if (model.list_practice.Where(p => p.Selected == true).Count() <= 0)
                return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse, error = true, errorText = "Необходимо выбрать хотя бы одну практику для распределения" });

            if (typeResponse == 1)
            {
                ResponseFromStudent responseFromStudent = new();
                responseFromStudent = db.ResponseFromStudents
                                        .Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization)
                                        .Include(p => p.Student).ThenInclude(p => p.User)
                                        .Include(p => p.Student).ThenInclude(p => p.Resume)
                                        .Include(p => p.Vacancy).ThenInclude(p => p.Organization)
                                        .Where(p => p.IdResponse == id).First();
                IdGroup = responseFromStudent.Student.GroupId;
                status = responseFromStudent.Status;
                IdOrganization = responseFromStudent.Vacancy.OrganizationId;
                IdStudent = responseFromStudent.StudentId;

                if (status != 3)
                    return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse, error = true, errorText = "Непредвиденная ошибка" });

                if (responseFromStudent.CommentOrganization == null && model.CommentAccept != null && model.CommentAccept?.Trim().Length > 0)
                    responseFromStudent.CommentOrganization = "Комментарий отдела производтсвенного обучения:\n" + model.CommentAccept.Trim();
                if (responseFromStudent.CommentOrganization != null && model.CommentAccept != null && model.CommentAccept?.Trim().Length > 0)
                    responseFromStudent.CommentOrganization += "\n\nКомментарий отдела производтсвенного обучения:\n" + model.CommentAccept.Trim();

                responseFromStudent.Status = 4;
                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(responseFromStudent.Student.User.Email, responseFromStudent.Vacancy.NameVacancy, responseFromStudent.Vacancy.Organization.NotFullNameOrganization, responseFromStudent.CommentOrganization, responseFromStudent.CommentStudent, responseFromStudent.Status, responseFromStudent.DateTimeCreate, 1);
                db.ResponseFromStudents.Update(responseFromStudent);
            }
            if (typeResponse == 2)
            {
                ResponseFromOrganization responseFromOrganization = new();
                responseFromOrganization = db.ResponseFromOrganizations
                        .Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization)
                        .Include(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p => p.User)
                        .Include(p => p.Vacancy).ThenInclude(p => p.Organization)
                        .Where(p => p.IdResponse == id).First();

                IdGroup = responseFromOrganization.Resume.Student.GroupId;
                status = responseFromOrganization.Status;
                IdOrganization = responseFromOrganization.Vacancy.OrganizationId;
                IdStudent = responseFromOrganization.Resume.StudentId;

                if (status != 3)
                    return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse, error = true, errorText = "Непредвиденная ошибка" });

                if (responseFromOrganization.CommentOrganization == null && model.CommentAccept != null && model.CommentAccept?.Trim().Length > 0)
                    responseFromOrganization.CommentOrganization = "Комментарий отдела производтсвенного обучения:\n" + model.CommentAccept.Trim();
                if (responseFromOrganization.CommentOrganization != null && model.CommentAccept != null && model.CommentAccept?.Trim().Length > 0)
                    responseFromOrganization.CommentOrganization += "\n\nКомментарий отдела производтсвенного обучения:\n" + model.CommentAccept.Trim();

                responseFromOrganization.Status = 4;
                _ = new EmailService(configuration).SendEmailWithStatusResponseFromStudent(responseFromOrganization.Resume.Student.User.Email, responseFromOrganization.Vacancy.NameVacancy, responseFromOrganization.Vacancy.Organization.NotFullNameOrganization, responseFromOrganization.CommentOrganization, responseFromOrganization.CommentStudent, responseFromOrganization.Status, responseFromOrganization.DateTimeCreate, 1);
                db.ResponseFromOrganizations.Update(responseFromOrganization);
            }

            foreach (var a in model.list_practice.Where(p=>p.Selected == true))
            {
                if (db.PracticeChartDistibutions.Where(p => p.PracticeId == a.ID_Practice && p.StudentId == IdStudent).Count() <= 0)
                {
                    PracticeChartDistibution practice = new();
                    practice.PracticeId = a.ID_Practice;
                    practice.OrganizationId = IdOrganization;
                    practice.StudentId = IdStudent;
                    db.Add(practice);
                }
                else
                {
                    PracticeChartDistibution practice = db.PracticeChartDistibutions.Where(p => p.PracticeId == a.ID_Practice && p.StudentId == IdStudent).First();
                    practice.OrganizationId = IdOrganization;
                    db.Update(practice);
                }
            }

            db.SaveChanges();

            return RedirectToAction("InfoResponse", "ManageResponses", new { id = id, typeResponse = typeResponse });
        }
    }
}

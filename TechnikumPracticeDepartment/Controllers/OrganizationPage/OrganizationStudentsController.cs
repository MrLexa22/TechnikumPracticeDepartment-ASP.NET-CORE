using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.Models.ModelsOrganizationPages;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Controllers.OrganizationPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class OrganizationStudentsController : Controller
    {
        public static bool IsTimeInGivenPeriods(DateTime timeToCheck, IEnumerable<Periods> periods)
        {
            bool result = periods?.Any(p => (timeToCheck >= p.Start || timeToCheck <= p.Start) && timeToCheck <= p.End) ?? false;
            return result;
        }
        private TechnikumPracticeDepartmentContext db;
        public OrganizationStudentsController(TechnikumPracticeDepartmentContext context)
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
        public IActionResult Index()
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");
            DistributionStudentWithPractices model = new DistributionStudentWithPractices();
            model.list = new List<ConcatinationStudentWithPractice>();
            var organization = db.Users.Include(p=>p.EmployeeOfOrganization).ThenInclude(p=>p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            var students_inOrganization = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                                                       Include(p=>p.Student).ThenInclude(p=>p.User).
                                                                       Include(p => p.Student).ThenInclude(p=>p.Group).ThenInclude(p=>p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p=>p.Practice).
            Where(p => p.OrganizationId == organization.IdOrganization && (p.Student.IsStudent == true || p.Student.IsStudent == null) && (p.Student.User.IsAvaliable == true || p.Student.User.IsAvaliable == null)).
            Select(a => new Students()
            {
                student = a.Student,
                IsEnded = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Student.Group.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation + 1)
                                    : (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation),
            }).
            Where(p=>p.IsEnded == false).
            ToList().DistinctBy(p=>p.student.IdStudent).ToList();

            foreach (var a in students_inOrganization)
            {
                ConcatinationStudentWithPractice element = new ConcatinationStudentWithPractice();
                element.student = a.student;

                var list_practices = a.student.Group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
                list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();

                var list_practicess = new List<StudentsInformationPractice>();
                foreach (var o in list_practices)
                {
                    if (db.PracticeChartDistibutions.Where(p => p.PracticeId == o.IdPractice && p.StudentId == a.student.IdStudent && p.OrganizationId == organization.IdOrganization).Count() > 0)
                    {
                        o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.Practice).ThenInclude(p => p.PracticeChartDistibutions)
                                                            .Include(p => p.PracticesChartDates)
                                                            .Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == a.student.GroupId).Count() > 0 && p.Practice.PracticeChartDistibutions.Where(p => p.OrganizationId == organization.IdOrganization).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                        if (o.PracticeCharts.Count() > 0)
                        {
                            StudentsInformationPractice practicesStudent = new StudentsInformationPractice();
                            practicesStudent.NamePractice = o.NamePractice;
                            practicesStudent.NameProfModule = o.NameProfModule;
                            practicesStudent.ID_Practice = o.IdPractice;
                            practicesStudent.Hours = o.PracticeCharts.First().Hours.ToString();
                            practicesStudent.list_periods = new List<PracticeChart>();

                            var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                            var list_dates = new List<PracticeChart>();

                            foreach (var practiceDates in o.PracticeCharts)
                            {
                                string[] dayes = new string[10];
                                string days = "";
                                string daysDB = practiceDates.DaysPractice;
                                if (daysDB == "БУДН")
                                {
                                    days = "Каждый будний день";
                                }
                                else
                                {
                                    dayes = daysDB.Split("; ");
                                    days = "Каждый:";
                                    foreach (var h in dayes)
                                    {
                                        if (h == "ПН")
                                            days += " понедельник;";
                                        if (h == "ВТ")
                                            days += " вторник;";
                                        if (h == "СР")
                                            days += " среда;";
                                        if (h == "ЧТ")
                                            days += " четверг;";
                                        if (h == "ПТ")
                                            days += " пятница;";
                                    }
                                }
                                List<PracticesChartDate> list_datess = new List<PracticesChartDate>();
                                foreach (var datesDates in practiceDates.PracticesChartDates)
                                {
                                    list_datess.Add(datesDates);
                                }
                                PracticeChart practicesChart = new PracticeChart();
                                practicesChart.DaysPractice = days;
                                practicesChart.PracticesChartDates = list_datess;
                                practicesStudent.list_periods.Add(practicesChart);
                            }

                            TimeOnly time = new TimeOnly(0, 0, 0);
                            List<PracticesChartDate> practiceDates_all = new List<PracticesChartDate>();
                            foreach (var g in practicesStudent.list_periods)
                            {
                                foreach (var k in g.PracticesChartDates)
                                {
                                    practiceDates_all.Add(k);
                                }
                            }

                            if (IsTimeInGivenPeriods(DateTime.Now, practiceDates_all.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                                practicesStudent.IsEnded = true;
                            else
                                practicesStudent.IsEnded = false;

                            list_practicess.Add(practicesStudent);
                        }
                    }
                }
                element.list_practice = list_practicess;
                element.list_practice = element.list_practice.OrderBy(p => p.list_periods.Select(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).First()).ToList();
                foreach(var k in element.list_practice)
                {
                    if (k.IsEnded == false)
                    {
                        model.list.Add(element);
                        break;
                    }
                }
            }
            model.list = model.list.OrderBy(p => p.student.User.SurnameUser).ToList();
            return View("~/Views/OrganizationPage/StudentsPracticeOrganization.cshtml", model);
        }
        public IActionResult InfoStudent(int IdStudent)
        {
            if (UpdateIn(3) == false)
                return RedirectToAction("Index", "Home");
            ConcatinationStudentWithPractice model = new ConcatinationStudentWithPractice();
            model.list_practice = new List<StudentsInformationPractice>();
            
            var organization = db.Users.Include(p => p.EmployeeOfOrganization).ThenInclude(p => p.Organization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First().EmployeeOfOrganization.Organization;

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            var students_inOrganization = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                                                       Include(p => p.Student).ThenInclude(p => p.User).
                                                                       Include(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
            Where(p => p.OrganizationId == organization.IdOrganization && (p.Student.IsStudent == true || p.Student.IsStudent == null) && (p.Student.User.IsAvaliable == true || p.Student.User.IsAvaliable == null)).
            Select(a => new Students()
            {
                student = a.Student,
                IsEnded = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? true : false) : (a.Student.Group.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation + 1)
                                    : (a.Student.Group.YearOfGraduation <= yearNow ? a.Student.Group.YearOfGraduation - a.Student.Group.YearStartEducation : yearNow - a.Student.Group.YearStartEducation),
            }).
            Where(p => p.IsEnded == false).
            ToList().
            DistinctBy(p => p.student.IdStudent).
            Where(p=>p.student.IdStudent == IdStudent).
            ToList();

            foreach (var a in students_inOrganization)
            {
                model.student = a.student;
                var list_practices = a.student.Group.PracticesChartGroups.
                                                     Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
                
                list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();

                var list_practicess = new List<StudentsInformationPractice>();
                foreach (var o in list_practices)
                {
                    if (db.PracticeChartDistibutions.Where(p => p.PracticeId == o.IdPractice && p.StudentId == a.student.IdStudent && p.OrganizationId == organization.IdOrganization).Count() > 0)
                    {
                        o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.Practice).ThenInclude(p => p.PracticeChartDistibutions)
                                                            .Include(p => p.PracticesChartDates)
                                                            .Where(p => p.PracticeId == o.IdPractice &&
                                                                   p.PracticesChartGroups.Where(p => p.GroupId == a.student.GroupId).Count() > 0 &&
                                                                   p.Practice.PracticeChartDistibutions.Where(p => p.OrganizationId == organization.IdOrganization).Count() > 0 &&
                                                                   p.PracticesChartDates.Count() > 0)
                                                            .ToList();
                        if (o.PracticeCharts.Count() > 0)
                        {
                            StudentsInformationPractice practicesStudent = new StudentsInformationPractice();
                            practicesStudent.NamePractice = o.NamePractice;
                            practicesStudent.NameProfModule = o.NameProfModule;
                            practicesStudent.ID_Practice = o.IdPractice;
                            practicesStudent.Hours = o.PracticeCharts.First().Hours.ToString();
                            practicesStudent.list_periods = new List<PracticeChart>();

                            var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                            var list_dates = new List<PracticeChart>();

                            foreach (var practiceDates in o.PracticeCharts)
                            {
                                string[] dayes = new string[10];
                                string days = "";
                                string daysDB = practiceDates.DaysPractice;
                                if (daysDB == "БУДН")
                                {
                                    days = "Каждый будний день";
                                }
                                else
                                {
                                    dayes = daysDB.Split("; ");
                                    days = "Каждый:";
                                    foreach (var h in dayes)
                                    {
                                        if (h == "ПН")
                                            days += " понедельник;";
                                        if (h == "ВТ")
                                            days += " вторник;";
                                        if (h == "СР")
                                            days += " среда;";
                                        if (h == "ЧТ")
                                            days += " четверг;";
                                        if (h == "ПТ")
                                            days += " пятница;";
                                    }
                                }
                                List<PracticesChartDate> list_datess = new List<PracticesChartDate>();
                                foreach (var datesDates in practiceDates.PracticesChartDates)
                                {
                                    list_datess.Add(datesDates);
                                }
                                PracticeChart practicesChart = new PracticeChart();
                                practicesChart.DaysPractice = days;
                                practicesChart.PracticesChartDates = list_datess;
                                practicesStudent.list_periods.Add(practicesChart);
                            }

                            TimeOnly time = new TimeOnly(0, 0, 0);
                            List<PracticesChartDate> practiceDates_all = new List<PracticesChartDate>();
                            foreach (var g in practicesStudent.list_periods)
                            {
                                foreach (var k in g.PracticesChartDates)
                                {
                                    practiceDates_all.Add(k);
                                }
                            }

                            if (IsTimeInGivenPeriods(DateTime.Now, practiceDates_all.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                                practicesStudent.IsEnded = true;
                            else
                                practicesStudent.IsEnded = false;

                            list_practicess.Add(practicesStudent);
                        }
                    }
                }
                model.list_practice = list_practicess;
                model.list_practice = model.list_practice.OrderBy(p => p.list_periods.Select(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).First()).ToList();
            }
            return PartialView("~/Views/OrganizationPage/_InfoStudentModal.cshtml", model);
        }
    }
}

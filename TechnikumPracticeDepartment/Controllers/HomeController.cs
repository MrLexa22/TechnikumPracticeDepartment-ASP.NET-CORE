using DocumentFormat.OpenXml.Spreadsheet;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Xml;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Controllers
{
    public class HomeController : Controller
    {
        public static bool IsTimeInGivenPeriods(DateTime timeToCheck, IEnumerable<Periods> periods)
        {
            bool result = periods?.Any(p => (timeToCheck >= p.Start || timeToCheck <= p.Start) && timeToCheck <= p.End) ?? false;
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
        public HomeController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
        }

        [ViewLayout("_Layout")]
        public IActionResult Index(ModelErrorWindow model)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("MainPage", "Home");
            return View(model);
        }

        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult Error()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult MainPage()
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(0) == false)
                return RedirectToAction("Index", "Home");
            MainPageModel model = new MainPageModel();
            List<NewsModel> news = new List<NewsModel>();
            HtmlWeb web = new HtmlWeb();
            try
            {
                HtmlDocument doc = web.Load(@"https://new2.rea.ru/news");

                HtmlNode[] hrefs = doc.DocumentNode.SelectNodes("//a[contains(@class, 'news-list-item-content')]").ToArray();
                HtmlNode[] dates = doc.DocumentNode.SelectNodes("//div[contains(@class, 'news-list-item-date')]").ToArray();
                HtmlNode[] title = doc.DocumentNode.SelectNodes("//div[contains(@class, 'news-list-item-title')]").ToArray();

                for (int i = 0; i < title.Length; i++)
                {
                    NewsModel n = new NewsModel();
                    n.Date = dates[i].InnerHtml;
                    n.title = title[i].InnerText;
                    n.hrefs = "https://рэу.рф" + hrefs[i].Attributes["href"].Value;
                    news.Add(n);
                }
            }
            catch { }
            if (UpdateIn(4) == true)
            {
                var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

                var StudentInfo = db.Students.
                    Include(p => p.PracticeChartDistibutions).ThenInclude(p => p.Organization).
                    Include(p => p.Group).ThenInclude(p => p.Specialization).
                    Where(p => p.UserId == userDB.IdUser).First();

                var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                    Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                    Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                    Where(p => p.IdGroup == StudentInfo.GroupId).FirstOrDefault();

                var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
                list_practices = list_practices.DistinctBy(p => p.IdPractice).ToList();

                model.list_practices = new List<PracticesStudentWithDistribution>();
                foreach (var o in list_practices)
                {
                    o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == StudentInfo.GroupId).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                    if (o.PracticeCharts.Count() > 0)
                    {
                        PracticesStudentWithDistribution practicesStudent = new PracticesStudentWithDistribution();
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
                        {
                            practicesStudent.IsEnded = true;
                        }
                        else
                        {
                            practicesStudent.IsEnded = false;
                        }

                        if (StudentInfo.PracticeChartDistibutions.Where(p => p.PracticeId == practicesStudent.ID_Practice).Count() <= 0)
                        {
                            Organization organ = new Organization();
                            organ.FullNameOrganization = "Не распределён";
                            practicesStudent.organization = organ;
                        }
                        else
                        {
                            var organization = db.PracticeChartDistibutions.Include(p => p.Organization).Where(p => p.PracticeId == practicesStudent.ID_Practice && p.StudentId == StudentInfo.IdStudent).First().Organization;
                            practicesStudent.organization = organization;
                        }
                        model.list_practices.Add(practicesStudent);
                    }
                }
                model.list_practices = model.list_practices.OrderBy(p => p.list_periods.Select(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).First()).ToList();
            }
            model.list_newsUniversity = news;
            return View(model);
        }
        public bool CheckPassword(string newPassword)
        {
            if (newPassword.Trim().Length < 8)
                return false;
            if (Regex.IsMatch(newPassword, "^.*(?=.{8,})(?=.*[a-zA-Z])(?=.*\\d)(?=.*[!#$%&? ]).*$"))
                return true;
            return false;
        }

        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult PersonalAccount(bool? IsUpdatedPassword)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(0) == false)
                return RedirectToAction("Index", "Home");
            PersonalAccountModels model = new PersonalAccountModels();
            var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();
            if (IsUpdatedPassword == true)
                model.IsUpdatesPassword = true;
            model.SurnameUser = userDB.SurnameUser;
            model.NameUser = userDB.NameUser;
            model.PatronymicnameUser = userDB.PatronymicNameUser;
            model.Email = userDB.Email;

            if (rolesUser.Where(p=>p.RoleId == 1).Count() > 0)
            {
                model.roles = "Администратор";
            }
            if (rolesUser.Where(p => p.RoleId == 2).Count() > 0)
            {
                model.roles += " и Сотрудник производственного отдела";
            }
            //Студент
            if (rolesUser.Where(p => p.RoleId == 4).Count() > 0)
            {
                var StudentInfo = db.Students.
                                    Include(p=>p.PracticeChartDistibutions).ThenInclude(p=>p.Organization).
                                    Include(p=>p.Group).ThenInclude(p=>p.Specialization).
                                    Where(p => p.UserId == userDB.IdUser).First();

                model.GroupName = StudentInfo.Group.NameGroup;
                model.SpecializationCode = StudentInfo.Group.Specialization.SpecializationCode;
                model.SpecializationName = StudentInfo.Group.Specialization.SpecializationName;
                model.PhoneNumber = StudentInfo.PhoneNumber;
                model.PathImageStudent = StudentInfo.ImageStudent;
                model.dateOfBirthday = StudentInfo.DateOfBirthday?.ToString();
            }
            //Сотрудник организации
            if (rolesUser.Where(p => p.RoleId == 3).Count() > 0)
            {
                var organization = db.EmployeeOfOrganizations.Include(p=>p.Organization).Where(p => p.UserId == userDB.IdUser).First();
                model.info_organization = organization.Organization;
            }
            return View(model);
        }

        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult UpdatePassword()
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(0) == false)
                return RedirectToAction("Index", "Home");

            return PartialView("~/Views/Home/_ChangePasswordModal.cshtml", new UpdatePasswordModel());
        }

        [HttpPost]
        public IActionResult UpdatePasswordPost(UpdatePasswordModel model)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(0) == false)
                return RedirectToAction("Index", "Home");
            
            if(ModelState.IsValid == true)
            {
                var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                userDB.Password = HashPassword.hashPassword(model.newPassword);
                db.Users.Update(userDB);
                db.SaveChanges();
            }

            return RedirectToAction("PersonalAccount","Home", new { IsUpdatedPassword = true});
        }
        public bool CheckPhoneNumber(int Id_Student, string? PhoneNumber)
        {
            try
            {
                string regex = @"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?[0-9]{3}-[0-9]{2}-[0-9]{2})";
                if (PhoneNumber == null)
                    return true;
                else if (PhoneNumber.Length == 0)
                    return true;
                else if (PhoneNumber != null)
                {
                    if (!Regex.IsMatch(PhoneNumber, regex))
                        return false;
                    if (db.Students.Where(p => p.PhoneNumber == PhoneNumber && p.IdStudent != Id_Student).Count() > 0)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static int GetAge(DateTime birthDate)
        {
            var now = DateTime.Today;
            return (int)(now.Year - birthDate.Year - 1 +
                ((now.Month > birthDate.Month || now.Month == birthDate.Month && now.Day >= birthDate.Day) ? 1 : 0));
        }
        public bool CheckDateOfBirthday(string? DateOfBirthday)
        {
            try
            {
                if (DateOfBirthday == null)
                    return true;
                var datebirth = DateOfBirthday.Split("-");
                DateTime datebirhtdate = new DateTime(Convert.ToInt16(datebirth[0]), Convert.ToInt16(datebirth[1]), Convert.ToInt16(datebirth[2]));
                if (GetAge(datebirhtdate) < 10 || GetAge(datebirhtdate) > 100)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult UpdateStudentProfile()
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            User userDB = db.Users.Include(p=>p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            UpdateStudentInfoModel model = new UpdateStudentInfoModel();
            model.Id_Student = userDB.Student.IdStudent;
            model.phoneNumber = userDB.Student.PhoneNumber;
            if (userDB.Student.DateOfBirthday != null)
            {
                var date = userDB.Student.DateOfBirthday?.ToString().Split(".");
                model.dateOfBirthday = date[2]+"-"+ date[1] + "-" + date[0];
            }
            return PartialView("~/Views/Home/_UpdateStudentBirthPhone.cshtml", model);
        }

        [HttpPost]
        public IActionResult UpdateStudentProfilePost(UpdateStudentInfoModel model)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid == true)
            {
                var userDB = db.Users.Include(p=>p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                userDB.Student.PhoneNumber = model.phoneNumber;
                if (model.dateOfBirthday != null)
                {
                    var datebirth = model.dateOfBirthday.Split("-");
                    DateOnly datebirhtdate = new DateOnly(Convert.ToInt16(datebirth[0]), Convert.ToInt16(datebirth[1]), Convert.ToInt16(datebirth[2]));
                    userDB.Student.DateOfBirthday = datebirhtdate;
                }
                else
                {
                    userDB.Student.DateOfBirthday = null;
                }

                db.Students.Update(userDB.Student);
                db.SaveChanges();
            }

            return RedirectToAction("PersonalAccount", "Home", new { IsUpdatedPassword = true });
        }
        
        [ViewLayout("_LayoutAuthenticatedUser")]
        public IActionResult UpdateStudentImage()
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            User userDB = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            updateImageStudent model = new updateImageStudent();
            model.FTTPPathImage = userDB.Student.ImageStudent;
            return PartialView("~/Views/Home/_UpdateImageStudent.cshtml", model);
        }

        [HttpPost]
        public IActionResult UpdateImageStudentPost(IFormFile uploadedFile)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                User userDB = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                userDB.Student.ImageStudent = userDB.Student.IdStudent.ToString() + "/" + userDB.Student.IdStudent.ToString() + System.IO.Path.GetExtension(uploadedFile.FileName);
                db.Update(userDB.Student);
                db.SaveChanges();
                SendFileToServer sendFileTo = new SendFileToServer(configuration);
                _ = sendFileTo.SendFileImageStudent(uploadedFile, userDB.Student.IdStudent.ToString(), System.IO.Path.GetExtension(uploadedFile.FileName));
            }
            return RedirectToAction("PersonalAccount", "Home");
        }
        public IActionResult RemoveStudentImage()
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return RedirectToAction("AgreeFZ", "AgreeFZ");
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            User userDB = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            if (userDB.Student.ImageStudent != null)
            {
                SendFileToServer sendFileTo = new SendFileToServer(configuration);
                sendFileTo.DeleteOldFile(userDB.Student.ImageStudent, 1);
                userDB.Student.ImageStudent = null;
                db.Update(userDB.Student);
                db.SaveChanges();
            }
            return RedirectToAction("PersonalAccount", "Home");
        }
    }
}
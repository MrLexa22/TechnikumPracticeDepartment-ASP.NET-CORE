using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Security.Claims;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace TechnikumPracticeDepartment.Controllers.StudentPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class StudentDocumentsPracticeController : Controller
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
        public static bool IsTimeInGivenPeriods(DateTime timeToCheck, IEnumerable<Periods> periods)
        {
            bool result = periods?.Any(p => (timeToCheck >= p.Start || timeToCheck <= p.Start) && timeToCheck <= p.End) ?? false;
            return result;
        }
        public string ChangeStringWithCav(string inputSTR)
        {
            try
            {
                int indexFirst = inputSTR.IndexOf('"');
                int indexLast = inputSTR.LastIndexOf('"');
                inputSTR = inputSTR.Remove(indexFirst, 1).Insert(indexFirst, "«");
                inputSTR = inputSTR.Remove(indexLast, 1).Insert(indexLast, "»");
            }
            catch { }
            return inputSTR;
        }

        TechnikumPracticeDepartmentContext db;
        public StudentDocumentsPracticeController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            PersonalAccountModels model = new PersonalAccountModels();
            var userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();

            if (rolesUser.Where(p => p.RoleId == 4).Count() > 0)
            {
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

                model.GroupName = StudentInfo.Group.NameGroup;
                model.SpecializationCode = StudentInfo.Group.Specialization.SpecializationCode;
                model.SpecializationName = StudentInfo.Group.Specialization.SpecializationName;
                model.PhoneNumber = StudentInfo.PhoneNumber;
                model.PathImageStudent = StudentInfo.ImageStudent;
                model.dateOfBirthday = StudentInfo.DateOfBirthday?.ToString();
                return View("~/Views/StudentPage/DocumentsPractice/Index.cshtml", model);
            }
            else
                return RedirectToAction("MainPage", "Home");
        }
        public IActionResult downloadAttList(int idPractice)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();

            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            var listPracticechart_all = db.PracticeCharts.Include(p => p.PracticesChartDates).
                                          Include(p => p.Practice).
                                          Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                          Where(p => p.PracticeId == idPractice && p.PracticesChartGroups.Where(p => p.GroupId == userDB.Student.GroupId).Count() > 0)
                                          .ToList();

            FileStream fileStreamPath = new FileStream(@"exmplesWord/DocsPractice/Аттестационный лист.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
            {
                var listPracticechart = listPracticechart_all.Where(p => p.PracticesChartGroups.Where(a => a.Group.SpecializationId == userDB.Student.Group.SpecializationId && a.GroupId == userDB.Student.GroupId).Count() > 0).ToList();
                listPracticechart = listPracticechart.OrderBy(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).ToList();

                document.Replace("<<STUDENT_FIO>>", userDB.PatronymicNameUser == null ? userDB.SurnameUser + " " + userDB.NameUser : userDB.SurnameUser + " " + userDB.NameUser + " " + userDB.PatronymicNameUser, true, true);

                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                int Course = (nowDate > augustDate) ? (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation + 1)
                                : (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation);
                document.Replace("<<STUDENT_COURSE>>", Course.ToString(), true, true);

                document.Replace("<<STUDENT_GROUP>>", userDB.Student.Group.NameGroup, true, true);

                document.Replace("<<STUDENT_SPECNAME>>", userDB.Student.Group.Specialization.SpecializationCode.Substring(0, 8) + " " + userDB.Student.Group.Specialization.SpecializationName + " (Квалификация: "+ userDB.Student.Group.Specialization.NameQualification+")", true, true);

                document.Replace("<<NAME_PRACTICE>>", ChangeStringWithCav(listPracticechart[0].Practice.NamePractice), true, true);

                document.Replace("<<NAME_PROFMODULE>>", ChangeStringWithCav(listPracticechart[0].Practice.NameProfModule), true, true);

                document.Replace("<<HOURS_PRACTICE>>", listPracticechart[0].Hours.ToString(), true, true);

                var practicePeriod = String.Join(";" + (listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).ToArray().Count() == 1 ? "" : "\n"), listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).Select(a => "С «" + a.DateStart.Day.ToString() + "» " + a.DateStart.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateStart.Year.ToString() + " г. по «" + a.DateEnd.Day.ToString() + "» " + a.DateEnd.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateEnd.Year.ToString() + " г.").ToArray());
                document.Replace("<<PERIOD_LIST>>", practicePeriod, true, true);

                var OrganizationInfo = db.PracticeChartDistibutions.Include(p => p.Organization).Where(p => p.PracticeId == listPracticechart[0].Practice.IdPractice && p.StudentId == userDB.Student.IdStudent).ToList();
                if (OrganizationInfo.Count() == 0)
                {
                    document.Replace("<<NAME_ORGANIZATION>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                    document.Replace("<<FIO_RUKOVODITELA>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                }
                else
                {
                    document.Replace("<<NAME_ORGANIZATION>>", ChangeStringWithCav(OrganizationInfo[0].Organization.FullNameOrganization), true, true);
                    document.Replace("<<FIO_RUKOVODITELA>>", OrganizationInfo[0].Organization.PatronymicContactOfOrganization == null ? OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization[0] + "." : OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization[0] + ". " + OrganizationInfo[0].Organization.PatronymicContactOfOrganization[0] + ".", true, true);
                }
                document.Replace("<<YEAR_NOW>>", DateTime.Now.Year.ToString(), true, true);

                stream = new MemoryStream();
                document.Save(stream, FormatType.Docx);
                document.Close();
                stream.Position = 0;
                stream = RemoveWatermarks.removeWatermarks(stream);
                if (listPracticechart[0].Practice.NamePractice.Substring(0, 3) != "ПДП")
                    return File(stream, "application/msword", "Shablon att list PP." + listPracticechart[0].Practice.NamePractice.Substring(3, 5) + ".docx");
                else
                    return File(stream, "application/msword", "Shablon att list PP PDP.docx");
            }
        }
        public IActionResult downloadDnevnik(int idPractice)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();

            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            var listPracticechart_all = db.PracticeCharts.Include(p => p.PracticesChartDates).
                                          Include(p => p.Practice).
                                          Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                          Where(p => p.PracticeId == idPractice && p.PracticesChartGroups.Where(p => p.GroupId == userDB.Student.GroupId).Count() > 0)
                                          .ToList();

            FileStream fileStreamPath = new FileStream(@"exmplesWord/DocsPractice/Дневник.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
            {
                var listPracticechart = listPracticechart_all.Where(p => p.PracticesChartGroups.Where(a => a.Group.SpecializationId == userDB.Student.Group.SpecializationId && a.GroupId == userDB.Student.GroupId).Count() > 0).ToList();
                listPracticechart = listPracticechart.OrderBy(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).ToList();

                document.Replace("<<FAMILIA_STUDENT>>", userDB.SurnameUser, true, true);
                document.Replace("<<IMA_STUDENT>>", userDB.NameUser, true, true);
                document.Replace("<<OTCH_STUDENT>>", userDB.PatronymicNameUser?.ToString(), true, true);

                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                int Course = (nowDate > augustDate) ? (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation + 1)
                                : (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation);
                document.Replace("<<COURSE>>", Course.ToString(), true, true);

                document.Replace("<<NAME_GROUP>>", userDB.Student.Group.NameGroup, true, true);

                document.Replace("<<SPEC_CODE>>", userDB.Student.Group.Specialization.SpecializationCode.Substring(0, 8), true, true);
                document.Replace("<<SPEC_NAME>>", userDB.Student.Group.Specialization.SpecializationName, true, true);
                document.Replace("<<QUALNAME>>", userDB.Student.Group.Specialization.NameQualification, true, true);

                document.Replace("<<NAME_PRACTICE>>", ChangeStringWithCav(listPracticechart[0].Practice.NamePractice), true, true);

                document.Replace("<<NAME_PROFMODULE>>", ChangeStringWithCav(listPracticechart[0].Practice.NameProfModule), true, true);

                document.Replace("<<HOURS_PRACTICE>>", listPracticechart[0].Hours.ToString(), true, true);

                var practicePeriod = String.Join(";" + (listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).ToArray().Count() == 1 ? "" : "\n"), listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).Select(a => "С «" + a.DateStart.Day.ToString() + "» " + a.DateStart.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateStart.Year.ToString() + " г. по «" + a.DateEnd.Day.ToString() + "» " + a.DateEnd.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateEnd.Year.ToString() + " г.").ToArray());
                document.Replace("<<LIST_PERIODS>>", practicePeriod, true, true);

                var OrganizationInfo = db.PracticeChartDistibutions.Include(p => p.Organization).Where(p => p.PracticeId == listPracticechart[0].Practice.IdPractice && p.StudentId == userDB.Student.IdStudent).ToList();
                if (OrganizationInfo.Count() == 0)
                {
                    document.Replace("<<FULLNAME_ORGANIZATION>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                    document.Replace("<<FIO_RUKOVODITEL>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                    document.Replace("<<ADDRESS_ORGANIZATION>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                }
                else
                {
                    document.Replace("<<FULLNAME_ORGANIZATION>>", ChangeStringWithCav(OrganizationInfo[0].Organization.FullNameOrganization), true, true);
                    document.Replace("<<FIO_RUKOVODITEL>>", OrganizationInfo[0].Organization.PatronymicContactOfOrganization == null ? OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization : OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization + " " + OrganizationInfo[0].Organization.PatronymicContactOfOrganization, true, true);
                    document.Replace("<<ADDRESS_ORGANIZATION>>", OrganizationInfo[0].Organization.AddressOrganization, true, true);
                }
                document.Replace("<<YEAR_NOW>>", DateTime.Now.Year.ToString(), true, true);
                document.Replace("<<STUDENT_INITIALS>>", userDB.SurnameUser+" " + userDB.NameUser[0]+"."+(userDB.PatronymicNameUser == null ? "" : " "+userDB.PatronymicNameUser[0]+"."), true, true);
                stream = new MemoryStream();
                document.Save(stream, FormatType.Docx);
                document.Close();
                stream.Position = 0;
                stream = RemoveWatermarks.removeWatermarks(stream);
                if (listPracticechart[0].Practice.NamePractice.Substring(0, 3) != "ПДП")
                    return File(stream, "application/msword", "Shablon dnevnik PP." + listPracticechart[0].Practice.NamePractice.Substring(3, 5) + ".docx");
                else
                    return File(stream, "application/msword", "Shablon dnevnik PP PDP.docx");
            }
        }
        public IActionResult downloadOtchet(int idPractice)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();

            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            var listPracticechart_all = db.PracticeCharts.Include(p => p.PracticesChartDates).
                                          Include(p => p.Practice).
                                          Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                          Where(p => p.PracticeId == idPractice && p.PracticesChartGroups.Where(p => p.GroupId == userDB.Student.GroupId).Count() > 0)
                                          .ToList();

            FileStream fileStreamPath = new FileStream(@"exmplesWord/DocsPractice/Отчёт.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
            {
                var listPracticechart = listPracticechart_all.Where(p => p.PracticesChartGroups.Where(a => a.Group.SpecializationId == userDB.Student.Group.SpecializationId && a.GroupId == userDB.Student.GroupId).Count() > 0).ToList();
                listPracticechart = listPracticechart.OrderBy(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).ToList();
                
                document.Replace("<<NAME_PRACTICE>>", ChangeStringWithCav(listPracticechart[0].Practice.NamePractice), true, true);
                document.Replace("<<NAME_PROFMODULE>>", ChangeStringWithCav(listPracticechart[0].Practice.NameProfModule), true, true);
                document.Replace("<<FIO_STUDENT>>", userDB.SurnameUser + " " + userDB.NameUser + " " + userDB.PatronymicNameUser?.ToString(), true, true);
                document.Replace("<<GROUP_NAME>>", userDB.Student.Group.NameGroup, true, true);

                document.Replace("<<SPEC_CODE>>", userDB.Student.Group.Specialization.SpecializationCode.Substring(0, 8), true, true);
                document.Replace("<<SPEC_NAME>>", userDB.Student.Group.Specialization.SpecializationName, true, true);
                document.Replace("<<QUAL_NAME>>", userDB.Student.Group.Specialization.NameQualification, true, true);

                document.Replace("<<HOURS_PRACTICE>>", listPracticechart[0].Hours.ToString(), true, true);

                var practicePeriod = String.Join(";" + (listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).ToArray().Count() == 1 ? "" : "\n"), listPracticechart[0].PracticesChartDates.OrderBy(p => p.DateEnd).Select(a => "С «" + a.DateStart.Day.ToString() + "» " + a.DateStart.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateStart.Year.ToString() + " г. по «" + a.DateEnd.Day.ToString() + "» " + a.DateEnd.ToString("MMM", CultureInfo.GetCultureInfo("ru")) + " " + a.DateEnd.Year.ToString() + " г.").ToArray());
                document.Replace("<<LIST_PERIODS>>", practicePeriod, true, true);

                var OrganizationInfo = db.PracticeChartDistibutions.Include(p => p.Organization).Where(p => p.PracticeId == listPracticechart[0].Practice.IdPractice && p.StudentId == userDB.Student.IdStudent).ToList();
                if (OrganizationInfo.Count() == 0)
                {
                    document.Replace("<<NAME_ORGANIZATION>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                    document.Replace("<<FIO_RUKOVODITEL>>", "НЕ РАСПРЕДЕЛЁН", true, true);
                }
                else
                {
                    document.Replace("<<NAME_ORGANIZATION>>", OrganizationInfo[0].Organization.FullNameOrganization, true, true);
                    document.Replace("<<FIO_RUKOVODITEL>>", OrganizationInfo[0].Organization.PatronymicContactOfOrganization == null ? OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization : OrganizationInfo[0].Organization.SurnameContactOfOrganization + " " + OrganizationInfo[0].Organization.NameContactOfOrganization + " " + OrganizationInfo[0].Organization.PatronymicContactOfOrganization, true, true);
                }
                document.Replace("<<YEAR_NOW>>", DateTime.Now.Year.ToString(), true, true);
                stream = new MemoryStream();
                document.Save(stream, FormatType.Docx);
                document.Close();
                stream.Position = 0;
                stream = RemoveWatermarks.removeWatermarks(stream);
                if (listPracticechart[0].Practice.NamePractice.Substring(0, 3) != "ПДП")
                    return File(stream, "application/msword", "Shablon otchet PP." + listPracticechart[0].Practice.NamePractice.Substring(3, 5) + ".docx");
                else
                    return File(stream, "application/msword", "Shablon otchet PP PDP.docx");
            }
        }
    }
}

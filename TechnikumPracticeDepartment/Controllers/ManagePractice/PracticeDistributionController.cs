using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsPracticeChartPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages;
using System.IO.Compression;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Linq;
using TechnikumPracticeDepartment.Controllers.StudentPage;

namespace TechnikumPracticeDepartment.Controllers.ManagePractice
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class PracticeDistributionController : Controller
    {
        public static Stream Zip(List<ZipItem> zipItems)
        {
            var zipStream = new MemoryStream();

            using (var zip = new System.IO.Compression.ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in zipItems)
                {
                    var entry = zip.CreateEntry(zipItem.Name);
                    using (var entryStream = entry.Open())
                    {
                        zipItem.Content.CopyTo(entryStream);
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }
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

        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public PracticeDistributionController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
        {
            db = context;
            configuration = conf;
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
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManagePractice/PracticeDistribution/Index.cshtml", db.Specializations.ToList());
        }
        public async Task<IActionResult> GetList(int? typeSortList, int? filterListBySpecializaion, int? filterListByCourse, int? filterListByAvaliable, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<ListDistribution> listGroups = new List<ListDistribution>();
            listGroups = db.Groups.Include(p=>p.Students).ThenInclude(p=>p.User).Include(p => p.Specialization).Include(p=>p.PracticesChartGroups).ThenInclude(p=>p.PracticeChart).ThenInclude(p=>p.Practice).Select(a => new ListDistribution()
            {
                ID_Group = a.IdGroup,
                NameGroups = a.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? true : false) : (a.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation + 1)
                                                : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                SpecializationName = a.Specialization.SpecializationCode + ". " + a.Specialization.SpecializationName,
                Specialization_ID = a.SpecializationId,
                YearStartEducation = a.YearStartEducation,
                YearOfGraduation = a.YearOfGraduation,
                list_students = a.Students.ToList(),
                list_practices = a.PracticesChartGroups.Select(p=>new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList(),
                count_students = a.Students.Where(p=>p.IsStudent == true || p.IsStudent == null).Count()
            }).ToList();

            foreach(var a in listGroups)
            {
                a.list_practices = a.list_practices.DistinctBy(p=>p.IdPractice).ToList();
            }

            int pageSize = 15;
            IQueryable<ListDistribution> Blocks = listGroups.AsQueryable();
            if (typeSortList == 1)
                Blocks = Blocks.OrderBy(p => p.NameGroups);
            else if (typeSortList == 2)
                Blocks = Blocks.OrderBy(p => p.Course);

            if (filterListBySpecializaion != 0 && filterListBySpecializaion != null)
                Blocks = Blocks.Where(p => p.Specialization_ID == filterListBySpecializaion);

            if (filterListByCourse != 0 && filterListByCourse != null)
                Blocks = Blocks.Where(p => p.Course == filterListByCourse);

            if (filterListByAvaliable == 0)
                Blocks = Blocks.Where(p=>p.IsEnded == false);
            if (filterListByAvaliable == 1)
                Blocks = Blocks.Where(p => p.IsEnded == true);

            if (!String.IsNullOrEmpty(search))
                Blocks = Blocks.Where(p => p.NameGroups.ToLower().Contains(search.ToLower()) || p.list_students.Where(p=>(p.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.User.NameUser.ToLower().Contains(search.ToLower())) && (p.IsStudent == null || p.IsStudent == true)).Count() > 0);

            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexDistributionStudents model = new IndexDistributionStudents();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_DistributionStudents(typeSortList, filterListBySpecializaion, filterListByCourse, filterListByAvaliable, search);
            model.list_groupsPractice = items;

            return PartialView("~/Views/ManagePractice/PracticeDistribution/_PracticeDistributionList.cshtml", model);
        }
        public IActionResult downloadDocxExport(int? filterListBySpecializaion, int? filterListByAvaliable, int? filterListByCourse)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();

            List<ListDistribution> listGroups = new List<ListDistribution>();
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            listGroups = db.Groups.Include(p => p.Students).ThenInclude(p => p.User)
                                  .Include(p => p.Specialization)
                                  .Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice)
            .Select(a => new ListDistribution()
            {
                ID_Group = a.IdGroup,
                NameGroups = a.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? true : false) : (a.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation + 1)
                                    : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                SpecializationName = a.Specialization.SpecializationCode + ". " + a.Specialization.SpecializationName,
                Specialization_ID = a.SpecializationId,
                YearStartEducation = a.YearStartEducation,
                YearOfGraduation = a.YearOfGraduation,
                specialization = a.Specialization,
                list_students = a.Students.Where(p => p.IsStudent == true || p.IsStudent == null).ToList(),
                list_practices = a.PracticesChartGroups.Select(p => p.PracticeChart.Practice).ToList(),
                count_students = a.Students.Where(p => p.IsStudent == true || p.IsStudent == null).Count()
            }).ToList();
            if (filterListByAvaliable == 0)
                listGroups = listGroups.Where(p => p.IsEnded == false).ToList();
            if (filterListByAvaliable == 1)
                listGroups = listGroups.Where(p => p.IsEnded == true).ToList();

            if(filterListBySpecializaion > 0)
                listGroups = listGroups.Where(p => p.Specialization_ID == filterListBySpecializaion).ToList();

            if (filterListByCourse > 0)
                listGroups = listGroups.Where(p => p.Course == filterListByCourse).ToList();

            List<Specialization> specializations = new List<Specialization>();
            foreach (var a in listGroups)
            {
                specializations.Add(a.specialization);
            }
            specializations = specializations.Distinct().ToList();

            List<ZipItem> zips = new List<ZipItem>();

            for (int j = 0; j < specializations.Count(); j++)
            {
                string specializaion_code = specializations[j].SpecializationCode;
                string specializaion_name = specializations[j].SpecializationName;
                string specializaion_qualification = specializations[j].NameQualification;

                var practicesGroupsSpecializaion_specializaion = listGroups.Where(p => p.Specialization_ID == specializations[j].IdSpecialization).ToList();

                for(int k = 0; k < 7; k++)
                {
                    var practicesGroupsSpecializaion = practicesGroupsSpecializaion_specializaion.Where(p => p.Course == k).ToList();
                    if(practicesGroupsSpecializaion.Count() > 0)
                    {
                        string course = k.ToString();
                        foreach(var a in practicesGroupsSpecializaion)
                        {
                            if(a.list_practices.Count() > 0)
                            {
                                a.list_practices = a.list_practices.Distinct().ToList();
                                foreach (var o in a.list_practices)
                                {
                                    o.PracticeCharts = db.PracticeCharts.Include(p=>p.PracticesChartGroups).Include(p=>p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p=>p.GroupId == a.ID_Group).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
                                    if (o.PracticeCharts.Count() > 0)
                                    {
                                        string name_practice = o.NamePractice;
                                        string name_profModule = o.NameProfModule;
                                        var dates = o.PracticeCharts.Select(p => p.PracticesChartDates).ToList();
                                        string perios = "";
                                        foreach (var dath in o.PracticeCharts)
                                        {
                                            foreach (var dath2 in dath.PracticesChartDates)
                                            {
                                                perios += "С " + dath2.DateStart.ToShortDateString() + " по " + dath2.DateEnd.ToShortDateString() + "; ";
                                            }
                                        }
                                        FileStream fileStreamPath = new FileStream(@"exmplesWord/Базы производственной практики.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                        using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
                                        {
                                            var listStudents_Group = db.Students.Include(p => p.User).Where(p => p.GroupId == a.ID_Group && (p.IsStudent == null || p.IsStudent == true)).ToList();
                                            if (listStudents_Group.Count() > 0)
                                            {
                                                listStudents_Group = listStudents_Group.OrderBy(p => p.User.SurnameUser).ToList();
                                                var distributedStudent = db.PracticeChartDistibutions.Include(p=>p.Organization).Where(p => p.PracticeId == o.PracticeCharts.ElementAt(0).Practice.IdPractice).ToList();
                                                document.Replace("<<NAME_PRACTICE>>", name_practice, true, true);
                                                document.Replace("<<NAME_PROFMODULE>>", name_profModule, true, true);
                                                document.Replace("<<CODE_SPECIALIZATION>>", specializaion_code, true, true);
                                                document.Replace("<<NAME_SPECIALIZAION>>", specializaion_name, true, true);
                                                document.Replace("<<NAME_QUALIFICATION>>", specializaion_qualification, true, true);
                                                document.Replace("<<COURSE>>", course, true, true);
                                                document.Replace("<<GROUPS_LIST>>", a.NameGroups, true, true);
                                                document.Replace("<<PERIOD_PRACTICE>>", perios, true, true);

                                                WTable table = new WTable(document);

                                                table.ResetCells(listStudents_Group.Count() + 1, 3);
                                                table[0, 0].AddParagraph().AppendText("Ф.И.О. студента-практиканта");
                                                table[0, 1].AddParagraph().AppendText("Наименование организации");
                                                table[0, 2].AddParagraph().AppendText("Контакты руководителя организации");

                                                WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
                                                tableStyle.TableProperties.Borders.Color = Syncfusion.Drawing.Color.Black;
                                                tableStyle.CellProperties.VerticalAlignment = VerticalAlignment.Middle;

                                                ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
                                                firstRowStyle.CharacterFormat.Italic = true;
                                                firstRowStyle.CellProperties.BackColor = Syncfusion.Drawing.Color.LightGray;
                                                firstRowStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                                                tableStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                                                tableStyle.ParagraphFormat.AfterSpacing = 0;

                                                table.TableFormat.Borders.LineWidth = 0.5f;
                                                table.TableFormat.Borders.Horizontal.LineWidth = 0.5f;
                                                table.TableFormat.Borders.Vertical.LineWidth = 0.5f;
                                                WTableRow row = table.Rows[0];
                                                table.ApplyStyle("CustomStyle");

                                                for (int i = 0; i < listStudents_Group.Count(); i++)
                                                {
                                                    string fio_student = listStudents_Group[i].User.SurnameUser + " " + listStudents_Group[i].User.NameUser[0] + ". " + (listStudents_Group[i].User.PatronymicNameUser == null ? "" : listStudents_Group[i].User.PatronymicNameUser?[0] + ".");
                                                    string name_organization = distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).Count() <= 0 ? "Не распределён" : distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.NotFullNameOrganization;
                                                    string contacts_organization = 
                                                        distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).Count() <= 0 ? "-" : distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.SurnameContactOfOrganization+" "
                                                        + distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.NameContactOfOrganization+" "
                                                        + (distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.PatronymicContactOfOrganization == null ? "" : distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.PatronymicContactOfOrganization)
                                                        + (distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.PhoneNumberContactOfOrganization == null ? "" : "\n"+distributedStudent.Where(p => p.StudentId == listStudents_Group[i].IdStudent).First().Organization.PhoneNumberContactOfOrganization);
                                                    table[i + 1, 0].AddParagraph().AppendText(fio_student);
                                                    table[i + 1, 1].AddParagraph().AppendText(name_organization);
                                                    table[i + 1, 2].AddParagraph().AppendText(contacts_organization);
                                                    if(contacts_organization == "-")
                                                    {
                                                        table[i + 1, 0].CellFormat.BackColor = Syncfusion.Drawing.Color.LightYellow;
                                                        table[i + 1, 1].CellFormat.BackColor = Syncfusion.Drawing.Color.LightYellow;
                                                        table[i + 1, 2].CellFormat.BackColor = Syncfusion.Drawing.Color.LightYellow;
                                                    }
                                                }

                                                TextBodyPart bodyPart = new TextBodyPart(document);
                                                bodyPart.BodyItems.Add(table);
                                                document.Replace("<<TABLE>>", bodyPart, true, true, true);

                                                String profmodeul = name_profModule.Substring(0, name_profModule.IndexOf(" \""));
                                                String practname = name_practice.Substring(0, name_practice.IndexOf(" \""));
                                                stream = new MemoryStream();
                                                document.Save(stream, FormatType.Docx);
                                                document.Close();
                                                stream.Position = 0;
                                                stream = RemoveWatermarks.removeWatermarks(stream);
                                                zips.Add(new ZipItem(specializaion_code + ". " + profmodeul + " (" + practname + ") " + a.NameGroups+" ("+a.Course.ToString()+" курс)" + ".docx", stream));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return File(Zip(zips), "application/octet-stream", "Raspredelenie PP.zip");
        }
        public IActionResult DistributionStudentsPage(int ID_Group)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var groups = db.Groups.Where(p => p.IdGroup == ID_Group).ToList();
            if(groups.Count() <= 0)
                return RedirectToAction("Index", "Home");
            DistributionStudentsPageModel model = new DistributionStudentsPageModel();

            var group = db.Groups.Include(p => p.Students).ThenInclude(p => p.User).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p => p.Practice).
                Include(p => p.PracticesChartGroups).ThenInclude(p => p.PracticeChart).ThenInclude(p=>p.PracticesChartDates).
                Where(p=>p.IdGroup == ID_Group).FirstOrDefault();
            group.Students = group.Students.Where(p => p.IsStudent == null || p.IsStudent == true).ToList();
            model.list_students = group.Students.OrderBy(p=>p.User.SurnameUser+" " + p.User.NameUser[0]).ToList();
            model.GroupName = group.NameGroup;

            var list_practices = group.PracticesChartGroups.Select(p => new Practice { IdPractice = p.PracticeChart.PracticeId, NamePractice = p.PracticeChart.Practice.NamePractice, NameProfModule = p.PracticeChart.Practice.NameProfModule }).ToList();
            list_practices = list_practices.DistinctBy(p=>p.IdPractice).ToList();
            model.list_practice = new List<Practice_DistributionStudent>();
            foreach (var o in list_practices)
            {
                o.PracticeCharts = db.PracticeCharts.Include(p => p.PracticesChartGroups).Include(p => p.PracticesChartDates).Where(p => p.PracticeId == o.IdPractice && p.PracticesChartGroups.Where(p => p.GroupId == ID_Group).Count() > 0 && p.PracticesChartDates.Count() > 0).ToList();
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
                    if(IsTimeInGivenPeriods(DateTime.Now, practic.list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) ==false)
                    {
                        practic.IsEnded = true;
                    }
                    else
                    {
                        practic.IsEnded = false;
                    }

                    practic.list_distributesStudents = db.PracticeChartDistibutions.Include(p => p.Student).ThenInclude(p => p.Group).Where(p => p.Student.Group.IdGroup == ID_Group).ToList();
                    model.list_practice.Add(practic);
                }
            }
            model.list_organization = db.Organizations.OrderBy(p=>p.NotFullNameOrganization).ToList();
            model.list_practice = model.list_practice.OrderBy(p => p.list_dates.Select(p => p.DateEnd).First()).ToList();
            return View("~/Views/ManagePractice/PracticeDistribution/DistributionStudents.cshtml", model);
        }

        [HttpPost]
        public async Task<Boolean> ChangeDistributionStudent(int ID_Practice, int ID_Student, int ID_Organization)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return false;

            string email_student = "";
            string NameModule = "";
            string NamePractice = "";
            string datesAndDaysPractice = "";
            string OrganizationInfo = "";
            string contactsOrganization = "";
            string addressOrganization = "";

            var student = db.Students.Include(p => p.User).Include(p=>p.Resume).Where(p => p.IdStudent == ID_Student).First();
            email_student = student.User.Email;
            var practiceInformation = db.PracticesChartGroups.
                                                    Include(p => p.PracticeChart).ThenInclude(p => p.Practice).
                                                    Include(p => p.PracticeChart).ThenInclude(p => p.PracticesChartDates).
                                                    Where(p => p.GroupId == student.GroupId && p.PracticeChart.PracticeId == ID_Practice).ToList();
            NameModule = practiceInformation.First().PracticeChart.Practice.NameProfModule;
            NamePractice = practiceInformation.First().PracticeChart.Practice.NamePractice;
            var list_dates = new List<PracticesChartDate>();
            foreach (var a in practiceInformation)
            {
                string days = "";
                if (a.PracticeChart.DaysPractice != "БУДН")
                {
                    string g = a.PracticeChart.DaysPractice;
                    string[] dayes = g.Split("; ");
                    days = "Каждый:";
                    foreach (var j in dayes)
                    {
                        if (j == "ПН")
                            days += " понедельник;";
                        if (j == "ВТ")
                            days += " вторник;";
                        if (j == "СР")
                            days += " среда;";
                        if (j == "ЧТ")
                            days += " четверг;";
                        if (j == "ПТ")
                            days += " пятница;";
                    }
                }
                else
                {
                    days = "Каждый будний день";
                }
                string dates = "";
                foreach (var j in a.PracticeChart.PracticesChartDates)
                {
                    dates += @$"<li>С {j.DateStart.ToShortDateString()} по {j.DateEnd}</li>";
                    list_dates.Add(j);
                }
                datesAndDaysPractice += @$"<div>{days}<br />{dates}</div>";
            }

            TimeOnly time = new TimeOnly(0, 0, 0);
            bool PracticIsEnded = false;
            if (IsTimeInGivenPeriods(DateTime.Now, list_dates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false)
                PracticIsEnded = true;
            else
                PracticIsEnded = false;

            if (ID_Organization == 0)
            {
                try
                {
                    var find_exist = db.PracticeChartDistibutions.Where(p => p.PracticeId == ID_Practice && p.StudentId == ID_Student).ToList();
                    if (find_exist.Count() > 0)
                    {
                        db.PracticeChartDistibutions.Remove(find_exist.First());
                        db.SaveChanges();
                        OrganizationInfo = "Не распределён!";
                        contactsOrganization = "Не распределён!";
                        addressOrganization = "Не распределён!";
                        _ = new EmailService(configuration).SendEmailDistribution(email_student, NameModule, NamePractice, datesAndDaysPractice, OrganizationInfo, contactsOrganization, addressOrganization);
                        if(PracticIsEnded == false)
                        {
                            if(student.Resume != null)
                            {
                                student.Resume.IsAvaliable = true;
                                db.Update(student.Resume);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    PracticeChartDistibution practiceChartDistibution = new PracticeChartDistibution();
                    var find_exist = db.PracticeChartDistibutions.Where(p => p.PracticeId == ID_Practice && p.StudentId == ID_Student).ToList();
                    if (find_exist.Count() > 0)
                    {
                        var find_exist_first = find_exist.First();
                        find_exist_first.OrganizationId = ID_Organization;
                        db.PracticeChartDistibutions.Update(find_exist_first);
                        if (PracticIsEnded == false)
                        {
                            if (student.Resume != null)
                            {
                                student.Resume.IsAvaliable = false;
                                db.Update(student.Resume);
                            }
                        }
                        db.SaveChanges();
                    }
                    else
                    {
                        practiceChartDistibution.OrganizationId = ID_Organization;
                        practiceChartDistibution.PracticeId = ID_Practice;
                        practiceChartDistibution.StudentId = ID_Student;
                        db.PracticeChartDistibutions.Add(practiceChartDistibution);
                        if (PracticIsEnded == false)
                        {
                            if (student.Resume != null)
                            {
                                student.Resume.IsAvaliable = false;
                                db.Update(student.Resume);
                            }
                        }
                        db.SaveChanges();
                    }
                    var organizationSelected = db.Organizations.Where(p => p.IdOrganization == ID_Organization).First();
                    OrganizationInfo = organizationSelected.FullNameOrganization + " ("+organizationSelected.NotFullNameOrganization+")";
                    contactsOrganization = organizationSelected.SurnameContactOfOrganization + " " + organizationSelected.NameContactOfOrganization + (organizationSelected.PatronymicContactOfOrganization == null ? "" : " "+organizationSelected.PatronymicContactOfOrganization)+(organizationSelected.PhoneNumberContactOfOrganization == null ? "" : " (Телефон: "+ organizationSelected.PhoneNumberContactOfOrganization+")");
                    addressOrganization = organizationSelected.AddressOrganization;
                    _ = new EmailService(configuration).SendEmailDistribution(email_student, NameModule, NamePractice, datesAndDaysPractice, OrganizationInfo, contactsOrganization, addressOrganization);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}

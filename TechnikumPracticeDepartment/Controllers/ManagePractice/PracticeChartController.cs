using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsPracticePages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.Models.ModelsPracticeChartPages;
using DocumentFormat.OpenXml.Drawing.Charts;
using OfficeOpenXml;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using MailKit;
using System.Drawing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Text;
using System.IO.Compression;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Finance;
using TechnikumPracticeDepartment.Controllers.StudentPage;

namespace TechnikumPracticeDepartment.Controllers.ManagePractice
{
    public class ZipItem
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
        public ZipItem(string name, Stream content)
        {
            this.Name = name;
            this.Content = content;
        }
        public ZipItem(string name, string contentStr, Encoding encoding)
        {
            // convert string to stream
            var byteArray = encoding.GetBytes(contentStr);
            var memoryStream = new MemoryStream(byteArray);
            this.Name = name;
            this.Content = memoryStream;
        }
    }
    public class Periods
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
    //Дни практики
    //ПН, ВТ, СР, ЧТ, ПТ
    //БУДН - каждый будний день
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class PracticeChartController : Controller
    {
        public static Stream Zip(List<ZipItem> zipItems)
        {
            var zipStream = new MemoryStream();

            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
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
        public PracticeChartController(TechnikumPracticeDepartmentContext context)
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
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<GroupsModel> listGroups = new List<GroupsModel>();
            listGroups = db.Groups.Include(p => p.Specialization).Select(a => new GroupsModel()
            {
                ID_Group = a.IdGroup,
                NameGroups = a.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? true : false) : (a.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation + 1)
                                                : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                SpecializationName = a.Specialization.SpecializationCode + ". " + a.Specialization.SpecializationName,
                Specialization_ID = a.SpecializationId,
                YearStartEducation = a.YearStartEducation,
                YearOfGraduation = a.YearOfGraduation
            }).ToList();
            listGroups = listGroups.Where(p => p.IsEnded == false).ToList();
            listGroups = listGroups.OrderBy(p => p.NameGroups).ThenBy(p => p.Course).ToList();
            return View("~/Views/ManagePractice/PracticeChart/Index.cshtml", listGroups);
        }
        public async Task<IActionResult> GetList(int? typeSortList, int? filterListByGroup, int? filterListByActivate, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var listPracticechart = db.PracticeCharts.Include(p => p.PracticesChartDates).
                                                      Include(p=>p.Practice).
                                                      Include(p=>p.PracticesChartGroups).ThenInclude(p=>p.Group).
                                                      ToList();

            int pageSize = 15;
            IQueryable<PracticeChart> Blocks = listPracticechart.AsQueryable();
            if (typeSortList == 1)
                Blocks = Blocks.OrderBy(p => p.Practice.NamePractice);
            else if (typeSortList == 2)
                Blocks = Blocks.OrderBy(p => p.Practice.NameProfModule);

            if (filterListByGroup != 0 && filterListByGroup != null)
                Blocks = Blocks.Where(p => p.PracticesChartGroups.Where(p => p.GroupId == filterListByGroup).Count() > 0);

            if(filterListByActivate == 0)
            {
                TimeOnly time = new TimeOnly(0, 0, 0);
                Blocks = Blocks.Where(p => IsTimeInGivenPeriods(DateTime.Now, p.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == true);
            }
            if (filterListByActivate == 1)
            {
                TimeOnly time = new TimeOnly(0, 0, 0);
                Blocks = Blocks.Where(p => IsTimeInGivenPeriods(DateTime.Now, p.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false);
            }

            if (!String.IsNullOrEmpty(search))
                Blocks = Blocks.Where(p => p.Practice.NamePractice.ToLower().Contains(search.ToLower()) || p.Practice.NameProfModule.ToLower().Contains(search.ToLower()));

            foreach(var a in Blocks)
            {
                if (a.DaysPractice != "БУДН")
                {
                    string g = a.DaysPractice;
                    string[] dayes = g.Split("; ");
                    a.DaysPractice = "Каждый:";
                    foreach (var j in dayes)
                    {
                        if (j == "ПН")
                            a.DaysPractice += " понедельник;";
                        if (j == "ВТ")
                            a.DaysPractice += " вторник;";
                        if (j == "СР")
                            a.DaysPractice += " среда;";
                        if (j == "ЧТ")
                            a.DaysPractice += " четверг;";
                        if (j == "ПТ")
                            a.DaysPractice += " пятница;";
                    }
                }
                else
                {
                    a.DaysPractice = "Каждый будний день";
                }
            }

            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexPracticeChartModel model = new IndexPracticeChartModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_PracticeChart(typeSortList, filterListByGroup, filterListByActivate, search);
            model.list_practiceChart = items;

            return PartialView("~/Views/ManagePractice/PracticeChart/_PracticeChartsList.cshtml", model);
        }
        public IActionResult downloadDocxExport(int? filterListByGroup, int? filterListByActivate)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();

            var listPracticechart_all = db.PracticeCharts.Include(p => p.PracticesChartDates).
                                          Include(p => p.Practice).
                                          Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                                          ToList();

            IQueryable<PracticeChart> Blocks = listPracticechart_all.AsQueryable();
            if (filterListByGroup != 0 && filterListByGroup != null)
            {
                var group = db.Groups.Where(p => p.IdGroup == filterListByGroup).FirstOrDefault();
                Blocks = Blocks.Where(p => p.PracticesChartGroups.Where(p => p.Group.SpecializationId == group.SpecializationId).Count() > 0);
            }

            if (filterListByActivate == 0)
            {
                TimeOnly time = new TimeOnly(0, 0, 0);
                Blocks = Blocks.Where(p => IsTimeInGivenPeriods(DateTime.Now, p.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == true);
            }
            if (filterListByActivate == 1)
            {
                TimeOnly time = new TimeOnly(0, 0, 0);
                Blocks = Blocks.Where(p => IsTimeInGivenPeriods(DateTime.Now, p.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == false);
            }
            listPracticechart_all = Blocks.ToList();

            List<Specialization> specializations = new List<Specialization>();
            foreach (var a in listPracticechart_all)
            {
                foreach (var j in a.PracticesChartGroups)
                {
                    specializations.Add(j.Group.Specialization);
                }
            }
            specializations = specializations.Distinct().ToList();

            List<ZipItem> zips = new List<ZipItem>();

            for (int j = 0; j < specializations.Count(); j++)
            {
                FileStream fileStreamPath = new FileStream(@"exmplesWord/График практик шаблон.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
                {
                    var listPracticechart = listPracticechart_all.Where(p => p.PracticesChartGroups.Where(a => a.Group.SpecializationId == specializations[j].IdSpecialization).Count() > 0).ToList();
                    listPracticechart = listPracticechart.OrderBy(p => p.PracticesChartDates.Select(p=>p.DateEnd).First()).ToList();
                    document.Replace("<<CODE_SPECIALIZATION>>", specializations[j].SpecializationCode, true, true);
                    document.Replace("<<NAME_SPECIALIZATION>>", specializations[j].SpecializationName + " (Квалификация: " + specializations[j].NameQualification+")", true, true);

                    WTable table = new WTable(document);

                    table.ResetCells(listPracticechart.Count() + 1, 4);
                    table[0, 0].AddParagraph().AppendText("Профессиональный модуль (ПМ), в рамках которого проводится производственная практика");
                    table[0, 1].AddParagraph().AppendText("Наименование производственной практики");
                    table[0, 2].AddParagraph().AppendText("Группы");
                    table[0, 3].AddParagraph().AppendText("Периоды проведения практики.\nДни практики");

                    for (int i = 0; i < listPracticechart.Count(); i++)
                    {
                        table[i + 1, 0].AddParagraph().AppendText(listPracticechart[i].Practice.NameProfModule);
                        table[i + 1, 1].AddParagraph().AppendText(listPracticechart[i].Practice.NamePractice);

                        var groups = String.Join(";\n", listPracticechart[i].PracticesChartGroups.Where(p => p.Group.SpecializationId == specializations[j].IdSpecialization).Select(a => a.Group.NameGroup).ToArray());
                        table[i + 1, 2].AddParagraph().AppendText(groups);

                        string days = "";
                        if (listPracticechart[i].DaysPractice == "БУДН")
                            days = "Каждый будний день (ПН-ПТ)";
                        else
                        {
                            days = "Практика проводится в следующие дни: ";
                            var days_db = listPracticechart[i].DaysPractice.Split("; ");
                            foreach(var l in days_db)
                            {
                                if (l == "ПН")
                                    days += "Понедельник; ";
                                if (l == "ВТ")
                                    days += "Вторник; ";
                                if (l == "СР")
                                    days += "Среда; ";
                                if (l == "ЧТ")
                                    days += "Четверг; ";
                                if (l == "ПТ")
                                    days += "Пятница; ";
                            }
                        }
                        var practicePeriod = String.Join(";\n", listPracticechart[i].PracticesChartDates.OrderBy(p=>p.DateEnd).Select(a => a.DateStart.ToShortDateString() + "-" + a.DateEnd.ToShortDateString()).ToArray());
                        table[i + 1, 3].AddParagraph().AppendText(practicePeriod+"\n"+ days);
                    }

                    WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
                    tableStyle.TableProperties.Borders.Color = Syncfusion.Drawing.Color.Black;
                    tableStyle.CellProperties.VerticalAlignment = VerticalAlignment.Middle;

                    ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
                    firstRowStyle.CharacterFormat.Italic = true;
                    firstRowStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                    tableStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                    tableStyle.ParagraphFormat.AfterSpacing = 0;

                    table.TableFormat.Borders.LineWidth = 0.5f;
                    table.TableFormat.Borders.Horizontal.LineWidth = 0.5f;
                    table.TableFormat.Borders.Vertical.LineWidth = 0.5f;
                    WTableRow row = table.Rows[0];
                    table.ApplyStyle("CustomStyle");

                    TextBodyPart bodyPart = new TextBodyPart(document);
                    bodyPart.BodyItems.Add(table);
                    document.Replace("<<TABLE>>", bodyPart, true, true, true);

                    stream = new MemoryStream();
                    document.Save(stream, FormatType.Docx);
                    document.Close();
                    stream.Position = 0;
                    stream = RemoveWatermarks.removeWatermarks(stream);
                    if(specializations.Count() == 1)
                        return File(stream, "application/msword", "Grafic PP " + specializations[j].SpecializationCode + ".docx");
                    zips.Add(new ZipItem("График ПП " + specializations[j].SpecializationCode + ".docx", stream));
                }
            }
            return File(Zip(zips), "application/octet-stream", "Grafics PP.zip");
        }
        public async Task<IActionResult> AddEditPracticeChart(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            AddEditPracticeChart model = new AddEditPracticeChart();
            model.list_practice = db.Practices.ToList();
            model.list_practice = model.list_practice.OrderBy(p => p.NamePractice).ToList();
            model.list_days = new List<DaysWithBool>();
            model.list_days.Add(new DaysWithBool(){ IsSelected = false, ShortNameDay = "ПН", NameDay = "Понедельник"});
            model.list_days.Add(new DaysWithBool() { IsSelected = false, ShortNameDay = "ВТ", NameDay = "Вторник" });
            model.list_days.Add(new DaysWithBool() { IsSelected = false, ShortNameDay = "СР", NameDay = "Среда" });
            model.list_days.Add(new DaysWithBool() { IsSelected = false, ShortNameDay = "ЧТ", NameDay = "Четверг" });
            model.list_days.Add(new DaysWithBool() { IsSelected = false, ShortNameDay = "ПТ", NameDay = "Пятница" });
            if (id != 0)
            {
                try
                {
                    var practicechart = db.PracticeCharts.Include(p => p.PracticesChartDates).Include(p => p.Practice).Where(p => p.IdChart == id).First();
                    model.SelectedIdPractice = practicechart.Practice.IdPractice;
                    model.ID_PracticeChart = practicechart.IdChart;
                    model.hours = practicechart.Hours.ToString();
                    model.list_dates = practicechart.PracticesChartDates.OrderBy(p=>p.DateEnd).ToList();
                    if (practicechart.DaysPractice != "БУДН")
                    {
                        foreach (var a in model.list_days)
                        {
                            string g = practicechart.DaysPractice;
                            if (g.Split("; ").Contains(a.ShortNameDay) == true)
                                a.IsSelected = true;
                        }
                    }
                    else
                    {
                        foreach (var a in model.list_days)
                        {
                            a.IsSelected = true;
                        }
                    }
                    model.NamePractice = practicechart.Practice.NamePractice + " (" + practicechart.Practice.NameProfModule + ")";
                    model.IsEndedPractice = IsTimeInGivenPeriods(DateTime.Now, practicechart.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(new TimeOnly(0, 0, 0)), End = a.DateEnd.ToDateTime(new TimeOnly(0, 0, 0)) }));
                }
                catch { }
            }
            else
            {
                model.IsEndedPractice = true;
                model.ID_PracticeChart = 0;
            }
            return View("~/Views/ManagePractice/PracticeChart/AddEditPracticeChart.cshtml", model);
        }
        public async Task<IActionResult> GetListGroups(int idPracticeChart, int selectedIdPractice)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            AddEditPracticeChart model = new AddEditPracticeChart();

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);

            if (idPracticeChart != 0)
            {
                var selectd_practiceChart = db.PracticeCharts.Include(p=>p.PracticesChartDates).Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).Where(p => p.IdChart == idPracticeChart).First();

                model.IsEndedPractice = IsTimeInGivenPeriods(DateTime.Now, selectd_practiceChart.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(new TimeOnly(0, 0, 0)), End = a.DateEnd.ToDateTime(new TimeOnly(0, 0, 0)) }));
                var practice = db.Practices.Include(p => p.PracticeSpecializations).ThenInclude(p => p.Specialization).ThenInclude(p => p.Groups).Where(p => p.IdPractice == selectedIdPractice).First();
                model.list_groups = new List<Models.ModelsSpecializationPages.GroupsSpecialization>();
                var practiceSpecializaions = practice.PracticeSpecializations;
                List<Group> listGroups = new List<Group>();
                foreach (var a in practice.PracticeSpecializations)
                {
                    foreach (var h in a.Specialization.Groups)
                    {
                        listGroups.Add(h);
                    }
                }

                List<GroupsModel> listGroups_forFilter = new List<GroupsModel>();
                listGroups_forFilter = listGroups.Select(a => new GroupsModel()
                {
                    ID_Group = a.IdGroup,
                    NameGroups = a.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? true : false) : (a.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation + 1)
                                                    : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                    SpecializationName = a.Specialization.SpecializationCode,
                    Specialization_ID = a.SpecializationId,
                    YearStartEducation = a.YearStartEducation,
                    YearOfGraduation = a.YearOfGraduation
                }).ToList();
                if(model.IsEndedPractice == true)
                    listGroups_forFilter = listGroups_forFilter.Where(p => p.IsEnded == false).ToList();
                if(selectedIdPractice != selectd_practiceChart.PracticeId)
                {
                    foreach (var a in listGroups_forFilter)
                    {
                        bool selecteds = false;
                        model.list_groups.Add(new Models.ModelsSpecializationPages.GroupsSpecialization() { ID_Group = a.ID_Group, NameGroup = a.NameGroups, SpecName = a.SpecializationName, Selected = selecteds, Course = a.Course });
                    }

                    model.list_groups = model.list_groups.OrderBy(p => p.NameGroup).ToList();
                }
                else
                {
                    foreach (var a in listGroups_forFilter)
                    {
                        bool selecteds = selectd_practiceChart.PracticesChartGroups.Where(p => p.GroupId == a.ID_Group).Count() > 0;
                        model.list_groups.Add(new Models.ModelsSpecializationPages.GroupsSpecialization() { ID_Group = a.ID_Group, NameGroup = a.NameGroups, SpecName = a.SpecializationName, Selected = selecteds, Course = a.Course });
                    }

                    model.list_groups = model.list_groups.OrderBy(p => p.NameGroup).ToList();
                }
            }
            else
            {
                model.IsEndedPractice = true;
                var practice = db.Practices.Include(p => p.PracticeSpecializations).ThenInclude(p => p.Specialization).ThenInclude(p => p.Groups).Where(p => p.IdPractice == selectedIdPractice).First();
                model.list_groups = new List<Models.ModelsSpecializationPages.GroupsSpecialization>();
                var practiceSpecializaions = practice.PracticeSpecializations;
                List<Group> listGroups = new List<Group>();
                foreach (var a in practice.PracticeSpecializations)
                {
                    foreach (var h in a.Specialization.Groups)
                    {
                        listGroups.Add(h);
                    }
                }
                List<GroupsModel> listGroups_forFilter = new List<GroupsModel>();
                listGroups_forFilter = listGroups.Select(a => new GroupsModel()
                {
                    ID_Group = a.IdGroup,
                    NameGroups = a.NameGroup,
                    IsEnded = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? true : false) : (a.YearOfGraduation < yearNow ? true : false),
                    Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation + 1)
                                                    : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                    SpecializationName = a.Specialization.SpecializationCode,
                    Specialization_ID = a.SpecializationId,
                    YearStartEducation = a.YearStartEducation,
                    YearOfGraduation = a.YearOfGraduation
                }).ToList();
                listGroups_forFilter = listGroups_forFilter.Where(p => p.IsEnded == false).ToList();
                foreach (var a in listGroups_forFilter)
                {
                    bool selecteds = false;
                    model.list_groups.Add(new Models.ModelsSpecializationPages.GroupsSpecialization() { ID_Group = a.ID_Group, NameGroup = a.NameGroups, SpecName = a.SpecializationName, Selected = selecteds, Course = a.Course });
                }
                model.list_groups = model.list_groups.OrderBy(p => p.NameGroup).ToList();
            }
            return PartialView("~/Views/ManagePractice/PracticeChart/_PracticeChartGroups.cshtml", model);
        }
        public bool CheckDoubleHours(string hours)
        {
            try
            {
                double hourss = Convert.ToDouble(hours);
                if (hourss > 2000 || hourss < 2)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckDateStartCreate(DateTime dateStartCreate, DateTime dateEndCreate)
        {
            if (dateEndCreate == null)
                return false;
            if (dateStartCreate == null)
                return false;
            if (dateStartCreate < DateTime.Now.AddMonths(-2))
                return false;
            if (dateStartCreate > DateTime.Now.AddMonths(12))
                return false;
            if (dateStartCreate >= dateEndCreate)
                return false;
            return true;
        }
        public bool CheckDateEndCreate(DateTime dateStartCreate, DateTime dateEndCreate)
        {
            if (dateEndCreate == null)
                return false;
            if (dateStartCreate == null)
                return false;
            if (dateEndCreate < DateTime.Now.AddMonths(-2))
                return false;
            if (dateEndCreate > DateTime.Now.AddMonths(12))
                return false;
            if (dateEndCreate <= dateStartCreate)
                return false;
            return true;
        }
        public bool CheckDateStart(DateTime dateStart, DateTime dateEnd, int ChartDates_ID, int ChartPractice_ID)
        {
            if (dateEnd == null)
                return false;
            if (dateStart == null)
                return false;
            if (dateStart < DateTime.Now.AddMonths(-2))
                return false;
            if (dateStart > DateTime.Now.AddMonths(12))
                return false;
            if (dateStart >= dateEnd)
                return false;
            var perios = db.PracticesChartDates.Where(p => p.IdDatePracticeChart != ChartDates_ID && p.PracticeChartId == ChartPractice_ID).Select(p=>new Periods() { Start = p.DateStart.ToDateTime(new TimeOnly(0,0,0)), End = p.DateEnd.ToDateTime(new TimeOnly(0, 0, 0)) });
            if (CheckIsTimeInGivenPeriods(dateStart, perios) == true)
                return false;
            return true;
        }
        public bool CheckDateEnd(DateTime dateStart, DateTime dateEnd, int ChartDates_ID, int ChartPractice_ID)
        {
            if (dateEnd == null)
                return false;
            if (dateStart == null)
                return false;
            if (dateEnd < DateTime.Now.AddMonths(-2))
                return false;
            if (dateEnd > DateTime.Now.AddMonths(12))
                return false;
            if (dateEnd <= dateStart)
                return false;
            var perios = db.PracticesChartDates.Where(p => p.IdDatePracticeChart != ChartDates_ID && p.PracticeChartId == ChartPractice_ID).Select(p => new Periods() { Start = p.DateStart.ToDateTime(new TimeOnly(0, 0, 0)), End = p.DateEnd.ToDateTime(new TimeOnly(0, 0, 0)) });
            if (CheckIsTimeInGivenPeriods(dateEnd, perios) == true)
                return false;
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> AddEditPracticeChartPost(int id, AddEditPracticeChart model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            if(model.list_groups == null)
            {
                return RedirectToAction("AddEditPracticeChart", "PracticeChart", new { id = id });
            }
            if(id == 0)
            {
                if (ModelState.IsValid == true)
                {
                    PracticeChart practiceChart = new PracticeChart();
                    practiceChart.PracticeId = model.SelectedIdPractice;
                    practiceChart.Hours = Convert.ToDouble(model.hours);
                    if (model.list_days.Where(p => p.IsSelected == true).Count() <= 0 || model.list_days.Where(p => p.IsSelected == true).Count() == 5)
                        practiceChart.DaysPractice = "БУДН";
                    else
                    {
                        int count_selectedDays = model.list_days.Where(p => p.IsSelected == true).Count();
                        for(int i = 0; i < count_selectedDays; i++)
                        {
                            if (i != count_selectedDays)
                                practiceChart.DaysPractice += model.list_days.Where(p => p.IsSelected == true).ToList()[i].ShortNameDay + "; ";
                            else
                                practiceChart.DaysPractice += model.list_days.Where(p => p.IsSelected == true).ToList()[i].ShortNameDay;
                        }
                    }
                    db.PracticeCharts.Add(practiceChart);
                    db.SaveChanges();

                    List<PracticesChartGroup> list_groups = new List<PracticesChartGroup>();
                    if (model.list_groups.Where(p => p.Selected == true).Count() <= 0)
                        list_groups.Add(new PracticesChartGroup() { GroupId = model.list_groups[0].ID_Group, PracticeChartId = practiceChart.IdChart });
                    else
                    {
                        foreach(var a in model.list_groups.Where(p => p.Selected == true))
                        {
                            list_groups.Add(new PracticesChartGroup() { GroupId = a.ID_Group, PracticeChartId = practiceChart.IdChart });
                        }
                    }
                    db.PracticesChartGroups.AddRange(list_groups);

                    List<PracticesChartDate> list_dates = new List<PracticesChartDate>();
                    list_dates.Add(new PracticesChartDate() { DateStart = new DateOnly(model.dateStartCreate.Year, model.dateStartCreate.Month, model.dateStartCreate.Day), DateEnd = new DateOnly(model.dateEndCreate.Year, model.dateEndCreate.Month, model.dateEndCreate.Day), PracticeChartId = practiceChart.IdChart });
                    db.PracticesChartDates.AddRange(list_dates);
                    db.SaveChanges();
                    id = practiceChart.IdChart;
                }
            }
            else
            {
                if(ModelState.ErrorCount < 3)
                {
                    PracticeChart practiceChart = db.PracticeCharts.Where(p => p.IdChart == id).First();
                    practiceChart.PracticeId = model.SelectedIdPractice;
                    practiceChart.Hours = Convert.ToDouble(model.hours);
                    if (model.list_days.Where(p => p.IsSelected == true).Count() <= 0 || model.list_days.Where(p => p.IsSelected == true).Count() == 5)
                        practiceChart.DaysPractice = "БУДН";
                    else
                    {
                        int count_selectedDays = model.list_days.Where(p => p.IsSelected == true).Count();
                        practiceChart.DaysPractice = "";
                        for (int i = 0; i < count_selectedDays; i++)
                        {
                            if (i != count_selectedDays)
                                practiceChart.DaysPractice += model.list_days.Where(p => p.IsSelected == true).ToList()[i].ShortNameDay + "; ";
                            else
                                practiceChart.DaysPractice += model.list_days.Where(p => p.IsSelected == true).ToList()[i].ShortNameDay;
                        }
                    }
                    db.PracticeCharts.Update(practiceChart);
                    db.SaveChanges();

                    var groups_for_remove = db.PracticesChartGroups.Where(p => p.PracticeChartId == id).ToList();
                    db.PracticesChartGroups.RemoveRange(groups_for_remove);
                    List<PracticesChartGroup> list_groups = new List<PracticesChartGroup>();
                    if (model.list_groups.Where(p => p.Selected == true).Count() <= 0)
                        list_groups.Add(new PracticesChartGroup() { GroupId = model.list_groups[0].ID_Group, PracticeChartId = practiceChart.IdChart });
                    else
                    {
                        foreach (var a in model.list_groups.Where(p => p.Selected == true))
                        {
                            list_groups.Add(new PracticesChartGroup() { GroupId = a.ID_Group, PracticeChartId = practiceChart.IdChart });
                        }
                    }
                    db.PracticesChartGroups.AddRange(list_groups);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("AddEditPracticeChart", "PracticeChart", new { id = id });
        }
        public async Task<IActionResult> DeletePracticeChart(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            if(id > 0)
            {
                var practicechart = db.PracticeCharts.Include(p => p.PracticesChartDates).Include(p => p.Practice).Where(p => p.IdChart == id).First();
                bool checker = IsTimeInGivenPeriods(DateTime.Now, practicechart.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(new TimeOnly(0, 0, 0)), End = a.DateEnd.ToDateTime(new TimeOnly(0, 0, 0)) }));
                if (checker == false)
                    return RedirectToAction("AddEditPracticeChart", "PracticeChart", new { id = id });
                db.PracticeCharts.Remove(practicechart);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "PracticeChart");
        }
        public async Task<IActionResult> AddEditDatePracticeChart(int idPracticeChart, int idChartDate)
        {
            AddEditPeriodPracticeChart model = new AddEditPeriodPracticeChart();
            model.ChartPractice_ID = idPracticeChart;
            model.ChartDates_ID = idChartDate;
            if(idChartDate > 0)
            {
                var dates = db.PracticesChartDates.Where(p => p.IdDatePracticeChart == idChartDate).First();
                model.dateStart = dates.DateStart.ToDateTime(new TimeOnly(0,0,0));
                model.dateEnd = dates.DateEnd.ToDateTime(new TimeOnly(0, 0, 0));
            }
            return PartialView("~/Views/ManagePractice/PracticeChart/_AddEditPracticeChartDates.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditDatePracticeChartPost(int idPracticeChart, int idChartDates, AddEditPeriodPracticeChart model)
        {
            if(ModelState.IsValid == true)
            {
                if(idChartDates == 0)
                {
                    var chartDates = new PracticesChartDate();
                    chartDates.DateStart = new DateOnly(model.dateStart.Year, model.dateStart.Month, model.dateStart.Day);
                    chartDates.DateEnd = new DateOnly(model.dateEnd.Year, model.dateEnd.Month, model.dateEnd.Day);
                    chartDates.PracticeChartId = idPracticeChart;
                    db.PracticesChartDates.Add(chartDates);
                    db.SaveChanges();
                }
                else
                {
                    var chartDates = db.PracticesChartDates.Find(idChartDates);
                    chartDates.DateStart = new DateOnly(model.dateStart.Year, model.dateStart.Month, model.dateStart.Day);
                    chartDates.DateEnd = new DateOnly(model.dateEnd.Year, model.dateEnd.Month, model.dateEnd.Day);
                    db.PracticesChartDates.Update(chartDates);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("AddEditPracticeChart", "PracticeChart",new {id = idPracticeChart});
        }
        public async Task<IActionResult> DeletePracticeChartDate(int id)
        {
            var practiceChartDate = db.PracticesChartDates.Find(id);
            if(db.PracticesChartDates.Where(p=>p.PracticeChartId == practiceChartDate.PracticeChartId).Count() <= 1)
                return RedirectToAction("AddEditPracticeChart", "PracticeChart", new { id = practiceChartDate.PracticeChartId });
            else
            {
                int idid = practiceChartDate.PracticeChartId;
                db.Remove(practiceChartDate);
                db.SaveChanges();
                return RedirectToAction("AddEditPracticeChart", "PracticeChart", new { id = idid });
            }
        }
    }
}

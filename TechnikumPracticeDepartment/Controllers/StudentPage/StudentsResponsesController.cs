using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsOrganizationPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsStudents;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using Spire.Doc;
using Spire.Doc.Documents;
using HeaderFooter = Spire.Doc.HeaderFooter;
using HeaderFooterType = Spire.Doc.Documents.HeaderFooterType;
using Break = Spire.Doc.Break;
using VerticalAlignment = Syncfusion.DocIO.DLS.VerticalAlignment;
using HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment;
using TextBodyPart = Syncfusion.DocIO.DLS.TextBodyPart;

namespace TechnikumPracticeDepartment.Controllers.StudentPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class StudentsResponsesController : Controller
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
        
        TechnikumPracticeDepartmentContext db;
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
        public StudentsResponsesController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/StudentPage/StudentsResponses/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? sortList, int? filterListType, int? filterListStatus, string search)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            IndexStudentsResponses model = new();
            var student = db.Students.Include(p => p.User).Where(p => p.User.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            model.responses_fromStudent = new();
            model.responses_fromOrganization = new();
            model.responses_fromStudent = db.ResponseFromStudents.Include(p=>p.Vacancy).ThenInclude(p=>p.Organization).Where(p => p.StudentId == student.IdStudent).ToList();
            model.responses_fromOrganization = db.ResponseFromOrganizations.Include(p=>p.Vacancy).ThenInclude(p=>p.Organization).Include(p=>p.Resume).ThenInclude(p=>p.Student).Where(p => p.Resume.Student.IdStudent == student.IdStudent).ToList();

            if(sortList > 0)
            {
                model.responses_fromStudent.OrderBy(p => p.DateTimeCreate).ToList();
                model.responses_fromOrganization.OrderBy(p => p.DateTimeCreate).ToList();
            }

            if(filterListType == 1)
                model.responses_fromOrganization = new();
            if (filterListType == 2)
                model.responses_fromStudent = new();

            if(filterListStatus > -1)
            {
                model.responses_fromStudent = model.responses_fromStudent.Where(p=>p.Status == filterListStatus).ToList();
                model.responses_fromOrganization = model.responses_fromOrganization.Where(p => p.Status == filterListStatus).ToList();
            }

            if (!String.IsNullOrEmpty(search))
            {
                model.responses_fromStudent = model.responses_fromStudent.Where(p => p.Vacancy.NameVacancy.ToLower().Contains(search.ToLower())).ToList();
                model.responses_fromOrganization = model.responses_fromOrganization.Where(p => p.Vacancy.NameVacancy.ToLower().Contains(search.ToLower())).ToList();
            }

            return PartialView("~/Views/StudentPage/StudentsResponses/_ResponsesList.cshtml", model);
        }
        public async Task<IActionResult> InfoResponse(int idResponse, int typeResponse, int fromDelete)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            var student = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            ResponseFromStudent fromStudent = new();
            ResponseFromOrganization fromOrganization = new();

            Vacancy vacancy = new();

            ConcatinationStudentWithPractice model = new();

            model.fromDeletet = fromDelete;
            //1 - Отклики
            //2 - Текущая вакансия

            int vacancyId = 0;

            if (typeResponse == 1)
            {
                fromStudent = db.ResponseFromStudents.Where(p => p.IdResponse == idResponse).First();
                if(fromStudent == null)
                    return RedirectToAction("Index", "Home");
                if(fromStudent.StudentId != student.Student.IdStudent)
                    return RedirectToAction("Index", "Home");
                vacancyId = fromStudent.VacancyId;
                model.typeResponse = 1;

            }
            if (typeResponse == 2)
            {
                fromOrganization = db.ResponseFromOrganizations.Include(p=>p.Resume).Where(p => p.IdResponse == idResponse).First();
                if (fromOrganization == null)
                    return RedirectToAction("Index", "Home");
                if (fromOrganization.Resume.StudentId != student.Student.IdStudent)
                    return RedirectToAction("Index", "Home");
                vacancyId = fromOrganization.VacancyId;
                model.typeResponse = 2;
            }

            vacancy = db.Vacancies.Include(p => p.ResponseFromStudents).
                ThenInclude(p => p.Student).ThenInclude(p => p.User).
                Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                Include(p => p.ResponseFromStudents).ThenInclude(p => p.Student).ThenInclude(p => p.Resume).
                Include(p=>p.Organization).
                Include(p => p.ResponseFromOrganizations).ThenInclude(p => p.Resume).ThenInclude(p => p.Student).ThenInclude(p =>p.Group).ThenInclude(p => p.Specialization).
                Where(p => p.IdVacancy == vacancyId).First();

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<StudentsReposnses> listStudents = new List<StudentsReposnses>();

            var responses = vacancy.ResponseFromStudents.Where(p => p.StudentId == student.Student.IdStudent).ToList();
            var responses2 = vacancy.ResponseFromOrganizations.Where(p => p.Resume.StudentId == student.Student.IdStudent).ToList();
            if (typeResponse == 1)
            {
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
            if (typeResponse == 2)
            {
                listStudents = responses2.Select(a => new StudentsReposnses()
                {
                    student = a.Resume.Student,
                    responseFromOrganization = a.Resume.ResponseFromOrganizations.Where(p=>p.IdResponse == idResponse).First(),
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
                    statusResponse = a.Status == 0 ? "создан" : (a.Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (a.Status == 2 ? "на рассмотрении (получен ответ студента)" : (a.Status == 3 ? "принят (на рассмотрении ОУ)" : (a.Status == 4 ? "принят" : (a.Status == 5 ? "отказ" : "отказ ОУ")))))
                }).ToList();
            }

            var studentInfo = listStudents.First();

            model.students = studentInfo;
            model.vacancy = vacancy;
            model.response = new();
            model.response2 = new();
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
            }

            return PartialView("~/Views/StudentPage/StudentsResponses/_InfoResponse.cshtml", model);
        }
        public IActionResult DeleteResponse(int idResponse, int fromDelete)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            var student = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            ResponseFromStudent fromStudent = new();
            int idRedirect = 0;
            if(db.ResponseFromStudents.Where(p=>p.IdResponse == idResponse && p.StudentId == student.Student.IdStudent).Count() > 0)
            {
                fromStudent = db.ResponseFromStudents.Where(p => p.IdResponse == idResponse && p.StudentId == student.Student.IdStudent).First();
                if (fromDelete == 1)
                    idRedirect = 0;
                else
                    idRedirect = fromStudent.VacancyId;
                db.ResponseFromStudents.Remove(fromStudent);
                db.SaveChanges();
            }
            if (fromDelete == 1)
                return RedirectToAction("Index", "StudentsResponses");
            if (fromDelete == 2)
                return RedirectToAction("LookVacancy", "ManageVacancy", new { id = idRedirect });

            return RedirectToAction("Index", "StudentsResponses");
        }
        public async Task<IActionResult> EditStatus(ConcatinationStudentWithPractice model, int id, int typeResponse)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            var student = db.Users.Include(p => p.Student).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            ResponseFromStudent fromStudent = new();
            ResponseFromOrganization fromOrganization = new();

            if(typeResponse == 1)
            {
                if (db.ResponseFromStudents.Where(p => p.IdResponse == id && p.StudentId == student.Student.IdStudent).Count() <= 0)
                    return NoContent();
                var response1 = db.ResponseFromStudents.Where(p => p.IdResponse == id && p.StudentId == student.Student.IdStudent).First();
                response1.Status = 2;
                response1.CommentStudent = model.response.CommentStudent;
                db.ResponseFromStudents.Update(response1);
                db.SaveChanges();
                return NoContent();
            }
            if(typeResponse == 2)
            {
                if (db.ResponseFromOrganizations.Include(p=>p.Resume).Where(p => p.IdResponse == id && p.Resume.StudentId == student.Student.IdStudent).Count() <= 0)
                    return NoContent();
                var response2 = db.ResponseFromOrganizations.Include(p => p.Resume).Where(p => p.IdResponse == id && p.Resume.StudentId == student.Student.IdStudent).First();
                response2.Status = model.response.Status;
                response2.CommentStudent = model.response.CommentStudent;
                db.ResponseFromOrganizations.Update(response2);
                db.SaveChanges();
                return NoContent();
            }

            return NoContent();
        }
        public IActionResult DistributionResponse()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            ResponseDistribution model = new ResponseDistribution();
            var student = db.Users.Include(p => p.Student).ThenInclude(p => p.RequestToDistributuion).ThenInclude(p=>p.EmployeeOfTechnikum).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            var RequestToDistribution = db.RequestToDistributuions.Where(p => p.StudentId == student.Student.IdStudent);
            if(RequestToDistribution.Count() > 0)
            {
                model.InnOrganization = student.Student.RequestToDistributuion.InnOrganization;
                model.AddressOrganization = student.Student.RequestToDistributuion.AddressOrganization;
                model.FullNameOrganization = student.Student.RequestToDistributuion.FullNameOrganization;
                model.NotFullNameOrganization = student.Student.RequestToDistributuion.NotFullNameOrganization;
                model.SurnameContactNameOrganization = student.Student.RequestToDistributuion.SurnameContactNameOrganization;
                model.NameContactNameOrganization = student.Student.RequestToDistributuion.NameContactNameOrganization;
                model.PatronymicNameContactNameOrganization = student.Student.RequestToDistributuion.PatronymicNameContactNameOrganization;
                model.Email = student.Student.RequestToDistributuion.EmailContactNameOrganization;
                model.PhoneNumber = student.Student.RequestToDistributuion.PhoneNumberContactNameOrganization;
                model.IdRequest = student.Student.RequestToDistributuion.IdRequest;
                model.StatusReuqest = student.Student.RequestToDistributuion.StatusReuqest;
                model.EmployeeOfTechnikum = student.Student.RequestToDistributuion.EmployeeOfTechnikum;
            }
            return View("~/Views/StudentPage/StudentsResponses/IndexDistributionResponse.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResponseDistribution(ResponseDistribution model)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return RedirectToAction("DistributionResponse", "StudentsResponses");

            var student = db.Users.Include(p => p.Student).ThenInclude(p=>p.RequestToDistributuion).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            var RequestToDistribution = db.RequestToDistributuions.Where(p => p.StudentId == student.Student.IdStudent);
            if (RequestToDistribution.Count() > 0)
                return RedirectToAction("DistributionResponse", "StudentsResponses");

            try
            {
                RequestToDistributuion request = new();
                request.InnOrganization = model.InnOrganization.Trim();
                request.AddressOrganization = model.AddressOrganization.Trim();
                request.SurnameContactNameOrganization = model.SurnameContactNameOrganization.Trim();
                request.NameContactNameOrganization = model.NameContactNameOrganization;
                request.PatronymicNameContactNameOrganization = model.PatronymicNameContactNameOrganization;
                request.NotFullNameOrganization = model.NotFullNameOrganization.Trim();
                request.FullNameOrganization = model.FullNameOrganization.Trim();
                request.EmailContactNameOrganization = model.Email;
                request.PhoneNumberContactNameOrganization = model.PhoneNumber;
                request.StudentId = student.Student.IdStudent;
                request.StatusReuqest = 0;

                db.RequestToDistributuions.Add(request);
                db.SaveChanges();
            }
            catch { }
            return RedirectToAction("DistributionResponse", "StudentsResponses");
        }
        public IActionResult downloadDogovor()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            var student = db.Users.Include(p=>p.Student).ThenInclude(p=>p.Group).ThenInclude(p=>p.Specialization).Include(p => p.Student).ThenInclude(p => p.RequestToDistributuion).ThenInclude(p => p.EmployeeOfTechnikum).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            var requestToDistributuion = db.RequestToDistributuions.Where(p => p.StudentId == student.Student.IdStudent).First();
            
            if (requestToDistributuion == null)
                return RedirectToAction("DistributionResponse", "StudentsResponses");
            if (requestToDistributuion.StatusReuqest == 0)
                return RedirectToAction("DistributionResponse", "StudentsResponses");
            MemoryStream stream = new MemoryStream();
            try
            {
                FileStream fileStreamPath = new FileStream(@"exmplesWord/Договор.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
                {
                    FileStream fileStream = new FileStream(@"exmplesWord/InformationForDogovor.txt", FileMode.Open);
                    string DIRECTOR_IM = "";
                    string DOVETN_MONTH = "";
                    string DOVERN_YEAR = "";
                    string DOVERN_DATE = "";
                    string DOVERN_NUMBER = "";
                    string INITIALSD = "";
                    string ADDRESS_UN = "";
                    string INN_UN = "";
                    string KPP_UN = "";
                    string OKTMO_UN = "";
                    string BANK_UN = "";
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        INITIALSD = reader.ReadLine();
                        INITIALSD += " "+reader.ReadLine()[0]+".";
                        string otch = reader.ReadLine();
                        INITIALSD += otch == "" ? "" : otch[0]+".";

                        DIRECTOR_IM = reader.ReadLine();
                        DIRECTOR_IM += " "+reader.ReadLine();
                        otch = reader.ReadLine();
                        DIRECTOR_IM += otch == "" ? "" : " "+otch;

                        string[] date = reader.ReadLine().Split(".");
                        DateTime dateTime = new DateTime(Convert.ToInt16(date[2]), Convert.ToInt16(date[1]), Convert.ToInt16(date[0]));
                        DOVETN_MONTH = dateTime.ToString("MMMM");
                        if(DOVETN_MONTH != "март" && DOVETN_MONTH != "август" && DOVETN_MONTH != "май")
                            DOVETN_MONTH = DOVETN_MONTH.Replace("ь", "я");
                        else if(DOVETN_MONTH != "март" || DOVETN_MONTH != "август")
                            DOVETN_MONTH = DOVETN_MONTH+"а";
                        else if(DOVETN_MONTH != "май")
                            DOVETN_MONTH = DOVETN_MONTH.Replace("й", "я");
                        DOVERN_YEAR = dateTime.Year.ToString();
                        DOVERN_DATE = dateTime.Day.ToString();

                        DOVERN_NUMBER = reader.ReadLine();
                        ADDRESS_UN = reader.ReadLine();
                        INN_UN = reader.ReadLine();
                        KPP_UN = reader.ReadLine();
                        OKTMO_UN = reader.ReadLine();
                        BANK_UN = reader.ReadLine();
                    }
                    document.Replace("<<YEAR_NOW>>", DateTime.Now.Year.ToString(), true, true);
                    document.Replace("<<INITIALSD>>", INITIALSD, true, true);
                    document.Replace("<<DIRECTOR_IM>>", DIRECTOR_IM, true, true);
                    document.Replace("<<DOVETN_MONTH>>", DOVETN_MONTH, true, true);
                    document.Replace("<<DOVERN_YEAR>>", DOVERN_YEAR, true, true);
                    document.Replace("<<DOVERN_DATE>>", DOVERN_DATE, true, true);
                    document.Replace("<<DOVERN_NUMBER>>", DOVERN_NUMBER, true, true);
                    document.Replace("<<ADDRESS_UN>>", ADDRESS_UN, true, true);
                    document.Replace("<<INN_UN>>", INN_UN, true, true);
                    document.Replace("<<KPP_UN>>", KPP_UN, true, true);
                    document.Replace("<<OKTMO_UN>>", OKTMO_UN, true, true);
                    document.Replace("<<BANK_UN>>", BANK_UN, true, true);

                    document.Replace("<<FULLNAME_ORGANIZATION>>", requestToDistributuion.FullNameOrganization, true, true);
                    document.Replace("<<ADDRESS_ORGANIZATION>>", requestToDistributuion.AddressOrganization, true, true);
                    document.Replace("<<INN_ORGANIZATION>>", requestToDistributuion.InnOrganization, true, true);

                    document.Replace("<<CODE_SPEC>>", student.Student.Group.Specialization.SpecializationCode, true, true);
                    document.Replace("<<NAME_SPEC>>", student.Student.Group.Specialization.SpecializationName, true, true);
                    document.Replace("<<QUAL_NAME>>", student.Student.Group.Specialization.NameQualification, true, true);
                    document.Replace("<<STUDENT_FIO_GROUP>>", student.SurnameUser+" "+student.NameUser+(student.PatronymicNameUser == null ? " "+student.Student.Group.NameGroup : " " + student.PatronymicNameUser + " " + student.Student.Group.NameGroup), true, true);

                    var listPracticechart_all = db.PracticeCharts.Include(p => p.PracticesChartDates).
                              Include(p => p.Practice).
                              Include(p => p.PracticesChartGroups).ThenInclude(p => p.Group).ThenInclude(p => p.Specialization).
                              Where(p=>p.PracticesChartGroups.Where(p=>p.GroupId == student.Student.GroupId).Count() > 0).
                              ToList();

                    var listPracticechart = listPracticechart_all.Where(p => p.PracticesChartGroups.Where(a => a.GroupId == student.Student.GroupId).Count() > 0).ToList();
                    TimeOnly time = new TimeOnly(0, 0, 0);
                    listPracticechart = listPracticechart.Where(p => IsTimeInGivenPeriods(DateTime.Now, p.PracticesChartDates.Select(a => new Periods { Start = a.DateStart.ToDateTime(time), End = a.DateEnd.ToDateTime(time) })) == true).ToList();
                    listPracticechart = listPracticechart.OrderBy(p => p.PracticesChartDates.Select(p => p.DateEnd).First()).ToList();
                    WTable table = new WTable(document);

                    table.ResetCells(listPracticechart.Count() + 1, 3);
                    table[0, 0].AddParagraph().AppendText("Профессиональный модуль (ПМ), в рамках которого проводится производственная практика");
                    table[0, 1].AddParagraph().AppendText("Наименование производственной практики");
                    table[0, 2].AddParagraph().AppendText("Периоды проведения практики.\nДни практики");

                    for (int i = 0; i < listPracticechart.Count(); i++)
                    {
                        table[i + 1, 0].AddParagraph().AppendText(listPracticechart[i].Practice.NameProfModule);
                        table[i + 1, 1].AddParagraph().AppendText(listPracticechart[i].Practice.NamePractice);

                        string days = "";
                        if (listPracticechart[i].DaysPractice == "БУДН")
                            days = "Каждый будний день (ПН-ПТ)";
                        else
                        {
                            days = "Практика проводится в следующие дни: ";
                            var days_db = listPracticechart[i].DaysPractice.Split("; ");
                            foreach (var l in days_db)
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
                        var practicePeriod = String.Join(";\n", listPracticechart[i].PracticesChartDates.OrderBy(p => p.DateEnd).Select(a => a.DateStart.ToShortDateString() + "-" + a.DateEnd.ToShortDateString()).ToArray());
                        table[i + 1, 2].AddParagraph().AppendText(practicePeriod + "\n" + days);
                    }
                    WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
                    tableStyle.TableProperties.Borders.Color = Syncfusion.Drawing.Color.Black;
                    tableStyle.CellProperties.VerticalAlignment = VerticalAlignment.Middle;

                    ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
                    firstRowStyle.CharacterFormat.Italic = true;
                    firstRowStyle.CharacterFormat.Bold = true;
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
                }

                stream = RemoveWatermarks.removeWatermarks(stream);
            }
            catch { }
            return File(stream, "application/msword", "Dogovor.docx");
        }
        public IActionResult deleteResponseDistribution()
        {
            var student = db.Users.Include(p => p.Student).ThenInclude(p => p.RequestToDistributuion).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            if(student.Student.RequestToDistributuion == null)
                return RedirectToAction("DistributionResponse", "StudentsResponses");
            db.RequestToDistributuions.Remove(student.Student.RequestToDistributuion);
            db.SaveChanges();
            return RedirectToAction("DistributionResponse", "StudentsResponses");
        }
    }
    public static class RemoveWatermarks
    {
        public static MemoryStream removeWatermarks(MemoryStream stream)
        {
            Document doc = new Document();
            doc.LoadFromStream(stream, FileFormat.Docx);
            doc.Replace("Created with a trial version of Syncfusion Word library", "", true, true);
            doc.Replace(" or registered the wrong key in your application. Click here to obtain the valid key.", "", true, true);
            Section section = doc.Sections[0];
            section.Paragraphs.RemoveAt(0);
            doc.SaveToStream(stream, FileFormat.Docx);
            doc.Close();
            stream.Position = 0;
            return stream;
        }
    }
}

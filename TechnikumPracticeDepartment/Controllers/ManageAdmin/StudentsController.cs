using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using System.Text.RegularExpressions;
using Group = TechnikumPracticeDepartment.ModelsDB.Group;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using System.Xml.Linq;
using TechnikumPracticeDepartment.Models.ModelsResumeStudent;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class StudentsController : Controller
    {
        public static int GetAge(DateTime birthDate)
        {
            var now = DateTime.Today;
            return (int)(now.Year - birthDate.Year - 1 +
                ((now.Month > birthDate.Month || now.Month == birthDate.Month && now.Day >= birthDate.Day) ? 1 : 0));
        }
        
        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public bool CheckPhoneNumber(int ID_Student, string? PhoneNumber)
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
                    if (db.Students.Where(p => p.PhoneNumber == PhoneNumber && p.IdStudent != ID_Student).Count() > 0)
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
        public bool CheckDateOfBirthday(string? DateOfBirthday)
        {
            try
            {
                if (DateOfBirthday == null)
                    return true;
                var datebirth = DateOfBirthday.Split("-");
                DateTime datebirhtdate = new DateTime(Convert.ToInt16(datebirth[0]), Convert.ToInt16(datebirth[1]), Convert.ToInt16(datebirth[2]));
                if (GetAge(datebirhtdate) < 14 || GetAge(datebirhtdate) > 80)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckDateOfBirthdayWithPoint(string? DateOfBirthday)
        {
            try
            {
                if (DateOfBirthday == null)
                    return true;
                var datebirth = DateOfBirthday.Split(".");
                DateTime datebirhtdate = new DateTime(Convert.ToInt16(datebirth[2]), Convert.ToInt16(datebirth[1]), Convert.ToInt16(datebirth[0]));
                if (GetAge(datebirhtdate) < 14 || GetAge(datebirhtdate) > 80)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public StudentsController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
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
            if (User.IsInRole("Не подтверждён ФЗ"))
                return false;
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
            return View("~/Views/ManageAdmin/Students/Index.cshtml", db.Groups.ToList().OrderBy(p=>p.NameGroup).ToList());
        }
        public async Task<IActionResult> GetList(int? typeSortListStudents, int? filterListStudentsByGroup, int? filterListStudentsByCourse, int? filterListStudentsByAvaliable, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<Students> listStudents = new List<Students>();
            listStudents = db.Students.Include(p => p.User).Include(p=>p.Resume).Include(p => p.Group).Select(a => new Students()
            {
                student = a,
                ID_Group = a.Group.IdGroup,
                NameGroups = a.Group.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.Group.YearOfGraduation <= yearNow ? true : false) : (a.Group.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.Group.YearOfGraduation <= yearNow ? a.Group.YearOfGraduation - a.Group.YearStartEducation : yearNow - a.Group.YearStartEducation + 1)
                                                    : (a.Group.YearOfGraduation <= yearNow ? a.Group.YearOfGraduation - a.Group.YearStartEducation : yearNow - a.Group.YearStartEducation),
                SpecializationName = a.Group.Specialization.SpecializationCode,
                Specialization_ID = a.Group.SpecializationId,
                YearStartEducation = a.Group.YearStartEducation,
                YearOfGraduation = a.Group.YearOfGraduation
            }).ToList();

            int pageSize = 15;
            IQueryable<Students> Blocks = listStudents.AsQueryable();
            if (typeSortListStudents == 1)
            {
                Blocks = Blocks.OrderBy(p => p.student.User.SurnameUser);
            }
            else if (typeSortListStudents == 2)
            {
                Blocks = Blocks.OrderBy(p => p.NameGroups);
            }
            else if (typeSortListStudents == 3)
            {
                Blocks = Blocks.OrderBy(p => p.student.User.Email);
            }

            if(filterListStudentsByGroup > 0)
            {
                Blocks = Blocks.Where(p => p.ID_Group == filterListStudentsByGroup);
            }

            if (filterListStudentsByCourse > 0)
            {
                Blocks = Blocks.Where(p => p.Course == filterListStudentsByCourse);
            }

            if (filterListStudentsByAvaliable == 1)
            {
                Blocks = Blocks.Where(p => (p.IsEnded == null || p.IsEnded == false) && (p.student.IsStudent == null || p.student.IsStudent == true));
            }
            else if (filterListStudentsByAvaliable == 2)
            {
                Blocks = Blocks.Where(p => (p.IsEnded == true) || (p.student.IsStudent == false));
            }

            if (!String.IsNullOrEmpty(search))
            {
                Blocks = Blocks.Where(p => p.student.User.SurnameUser.ToLower().Contains(search.ToLower()) || p.student.User.NameUser.ToLower().Contains(search.ToLower()));
            }
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexStudentsModel model = new IndexStudentsModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Students(typeSortListStudents, filterListStudentsByGroup, filterListStudentsByCourse, filterListStudentsByAvaliable, search);
            model.list_students = items;

            return PartialView("~/Views/ManageAdmin/Students/_StudentsList.cshtml", model);
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Студенты");
            List<ExportModelStudents> listExmple = new List<ExportModelStudents>();
            listExmple.Add(new ExportModelStudents
            {
                Email = "isip_d.rusakov@mpt.ru",
                SurnameStudent = "Русаков",
                NameStudent = "Денис",
                PatronymicNameStudent = null,
                GroupName = "П50-1-20",
                DateBirth = null,
                PhoneNumber = null,
            });
            listExmple.Add(new ExportModelStudents
            {
                Email = "isip_a.r.romanova@mpt.ru",
                SurnameStudent = "Романова",
                NameStudent = "Агрипина",
                PatronymicNameStudent = "Романовна",
                GroupName = "П50-1-21",
                DateBirth = null,
                PhoneNumber = null,
            });
            listExmple.Add(new ExportModelStudents
            {
                Email = "Iljya149@mpt.ru",
                SurnameStudent = "Юдин",
                NameStudent = "Илья",
                PatronymicNameStudent = "Маркович",
                GroupName = "БД50-1-19",
                DateBirth = "05.03.2002",
                PhoneNumber = "+7(914)234-58-09",
            });
            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file students.xlsx");
        }
        public IActionResult downloadExportStudents(int format, int? typeSortListStudents, int? filterListStudentsByGroup, int? filterListStudentsByCourse, int? filterListStudentsByAvaliable)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<Students> listStudents = new List<Students>();
            listStudents = db.Students.Include(p => p.User).Include(p => p.Group).Select(a => new Students()
            {
                student = a,
                ID_Group = a.Group.IdGroup,
                NameGroups = a.Group.NameGroup,
                IsEnded = (nowDate > augustDate) ? (a.Group.YearOfGraduation <= yearNow ? true : false) : (a.Group.YearOfGraduation < yearNow ? true : false),
                Course = (nowDate > augustDate) ? (a.Group.YearOfGraduation <= yearNow ? a.Group.YearOfGraduation - a.Group.YearStartEducation : yearNow - a.Group.YearStartEducation + 1)
                                                    : (a.Group.YearOfGraduation <= yearNow ? a.Group.YearOfGraduation - a.Group.YearStartEducation : yearNow - a.Group.YearStartEducation),
                SpecializationName = a.Group.Specialization.SpecializationCode,
                Specialization_ID = a.Group.SpecializationId,
                YearStartEducation = a.Group.YearStartEducation,
                YearOfGraduation = a.Group.YearOfGraduation
            }).ToList();

            IQueryable<Students> Blocks = listStudents.AsQueryable();
            if (typeSortListStudents == 1)
                Blocks = Blocks.OrderBy(p => p.student.User.SurnameUser);
            else if (typeSortListStudents == 2)
                Blocks = Blocks.OrderBy(p => p.NameGroups);
            else if (typeSortListStudents == 3)
                Blocks = Blocks.OrderBy(p => p.student.User.Email);

            if (filterListStudentsByGroup > 0)
                Blocks = Blocks.Where(p => p.ID_Group == filterListStudentsByGroup);

            if (filterListStudentsByCourse > 0)
                Blocks = Blocks.Where(p => p.Course == filterListStudentsByCourse);

            if (filterListStudentsByAvaliable == 1)
                Blocks = Blocks.Where(p => (p.IsEnded == null || p.IsEnded == false) && (p.student.IsStudent == null || p.student.IsStudent == true));
            else if (filterListStudentsByAvaliable == 2)
                Blocks = Blocks.Where(p => (p.IsEnded == true) || (p.student.IsStudent == false));
            
            listStudents = Blocks.ToList();

            var Students = listStudents.Select(p => new ExportModelStudents()
            {
                Email = p.student.User.Email,
                SurnameStudent = p.student.User.SurnameUser,
                NameStudent = p.student.User.NameUser,
                PatronymicNameStudent = p.student.User.PatronymicNameUser,
                GroupName = p.student.Group.NameGroup,
                DateBirth = p.student.DateOfBirthday?.ToString(),
                PhoneNumber = p.student.PhoneNumber?.ToString(),
            });
            if (format == 1)
            {
                System.IO.MemoryStream content = new System.IO.MemoryStream();
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    csv.WriteRecords(Students);
                }
                return File(content.ToArray(), "text/csv", "Export students " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Группы");
                ws.Cells["A1"].LoadFromCollection(Students, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export students " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public async Task<IActionResult> AddEditStudent(int id, string? recovery)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (recovery != null)
            {
                if (recovery == "T")
                    ViewData["Recovery"] = "T";
                if (recovery == "T1")
                    ViewData["Recovery"] = "T1";
            }

            AddEditStudent model = new AddEditStudent();
            model.list_groups = db.Groups.ToList().OrderBy(p=>p.NameGroup).ToList();
            if (id != 0)
            {
                var student = db.Students.Include(p => p.User).Include(p=>p.Resume).Include(p=>p.Group).Where(p => p.IdStudent == id).First();

                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                model.IsEnded = (nowDate > augustDate) ? (student.Group.YearOfGraduation <= yearNow ? true : false) : (student.Group.YearOfGraduation < yearNow ? true : false);
                model.IsStudent = student.IsStudent;
                model.DateOfBirthday = student.DateOfBirthday?.ToString();
                model.PathImageStudent = student.ImageStudent;
                model.SelectedGroup_ID = student.GroupId;
                model.ID_Student = student.IdStudent;
                model.Email = student.User.Email;
                model.SurnameUser = student.User.SurnameUser;
                model.NameUser = student.User.NameUser;
                model.PhoneNumber = student.PhoneNumber;
                model.PatronymicNameUser = student.User.PatronymicNameUser;
                model.ID_User = student.UserId;
                model.resum_is = student.Resume == null ? false : true;
            }
            else
            {
                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                List<Group> grouptoRemove = new List<Group>();
                foreach(var p in model.list_groups)
                {
                    var isEn = (nowDate > augustDate) ? (p.YearOfGraduation <= yearNow ? true : false) : (p.YearOfGraduation < yearNow ? true : false);
                    if (isEn == true)
                        grouptoRemove.Add(p);
                }
                foreach (var item in grouptoRemove) 
                    model.list_groups.Remove(item);
                model.ID_Student = 0;
            }

            return View("~/Views/ManageAdmin/Students/AddEditStudent.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditStudentPost(int id, AddEditStudent model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    User user = new User();
                    user = db.Users.Find(model.ID_User);
                    user.SurnameUser = ToUpperFirstLetter.UpperFirstLetter(model.SurnameUser.Trim());
                    user.NameUser = ToUpperFirstLetter.UpperFirstLetter(model.NameUser.Trim());
                    if (model.PatronymicNameUser != null)
                        user.PatronymicNameUser = ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameUser.Trim());
                    else
                        user.PatronymicNameUser = null;
                    user.Email = model.Email;
                    db.Users.Update(user);

                    Student student = new Student();
                    student = db.Students.Find(id);
                    student.GroupId = model.SelectedGroup_ID;
                    student.PhoneNumber = model.PhoneNumber;
                    student.DateOfBirthday = model.DateOfBirthday == null ? null : new DateOnly(Convert.ToInt16(model.DateOfBirthday.Split("-")[0]), Convert.ToInt16(model.DateOfBirthday.Split("-")[1]), Convert.ToInt16(model.DateOfBirthday.Split("-")[2]));
                    db.Students.Update(student);
                    db.SaveChanges();
                }
            }
            else
            {
                User user = new User();
                user.SurnameUser = ToUpperFirstLetter.UpperFirstLetter(model.SurnameUser.Trim());
                user.NameUser = ToUpperFirstLetter.UpperFirstLetter(model.NameUser.Trim());
                if (model.PatronymicNameUser != null)
                    user.PatronymicNameUser = ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameUser.Trim());
                else
                    user.PatronymicNameUser = null;
                user.Email = model.Email;
                string password = HashPassword.GetRandomPassword(8);
                user.Password = HashPassword.hashPassword(password);
                _ = new EmailService(configuration).SendEmailRegistrationAsync(user.Email, password, 1);
                db.Users.Add(user);
                db.SaveChanges();
                UsersRole role = new UsersRole() { RoleId = 4, UserId = user.IdUser };
                db.UsersRoles.Add(role);

                Student student = new Student();
                student.UserId = user.IdUser;
                student.GroupId = model.SelectedGroup_ID;
                student.PhoneNumber = model.PhoneNumber;
                student.DateOfBirthday = model.DateOfBirthday == null ? null : new DateOnly(Convert.ToInt16(model.DateOfBirthday.Split("-")[0]), Convert.ToInt16(model.DateOfBirthday.Split("-")[1]), Convert.ToInt16(model.DateOfBirthday.Split("-")[2]));
                db.Students.Add(student);
                db.SaveChanges();
                id = student.IdStudent;
                return RedirectToAction("AddEditStudent", "Students", new { id = id, recovery = "T1" });
            }
            return RedirectToAction("AddEditStudent", "Students", new { id = id });
        }
        public async Task<IActionResult> RemoveStudentImage(int id)
        {
            bool roleAdmin = UpdateIn(1);
            bool roleEmployee = UpdateIn(2);

            if(roleAdmin == false && roleEmployee == false)
                return RedirectToAction("Index", "Home");

            if (id>0)
            {
                var student = db.Students.Find(id);
                if(student.ImageStudent == null)
                    return RedirectToAction("AddEditStudent", "Students", new { id = id });

                string pathImage = student.ImageStudent;
                student.ImageStudent = null;
                db.Students.Update(student);
                db.SaveChanges();
                SendFileToServer sendFileTo = new SendFileToServer(configuration);
                sendFileTo.DeleteOldFile(pathImage, 1);
            }

            return RedirectToAction("AddEditStudent", "Students", new { id = id });
        }
        public async Task<IActionResult> RecoveryPassword(int id)
        {
            bool roleAdmin = UpdateIn(1);
            bool roleEmployee = UpdateIn(2);

            if (roleAdmin == false && roleEmployee == false)
                return RedirectToAction("Index", "Home");

            User user = new User();
            user = db.Users.Include(p=>p.Student).Where(p=>p.IdUser == id).First();
            string password = HashPassword.GetRandomPassword(8);
            user.Password = HashPassword.hashPassword(password);
            db.Users.Update(user);
            db.SaveChanges();
            _ = new EmailService(configuration).SendEmailRegistrationAsync(user.Email, password, 2);
            return RedirectToAction("AddEditStudent", "Students", new { id = user.Student.IdStudent, recovery = "T" });
        }
        public async Task<IActionResult> DeductStudent(int id)
        {
            bool roleAdmin = UpdateIn(1);
            bool roleEmployee = UpdateIn(2);

            if (roleAdmin == false && roleEmployee == false)
                return RedirectToAction("Index", "Home");

            Student student = new Student();
            student = db.Students.Include(p => p.User).Where(p => p.IdStudent == id).First();
            student.IsStudent = false;
            student.User.IsAvaliable = false;
            db.Students.Update(student);
            db.Users.Update(student.User);
            db.SaveChanges();
            return RedirectToAction("AddEditStudent", "Students", new { id = student.IdStudent});
        }
        public async Task<IActionResult> RecoveryStudent(int id)
        {
            bool roleAdmin = UpdateIn(1);
            bool roleEmployee = UpdateIn(2);

            if (roleAdmin == false && roleEmployee == false)
                return RedirectToAction("Index", "Home");

            Student student = new Student();
            student = db.Students.Include(p => p.User).Where(p => p.IdStudent == id).First();
            student.IsStudent = null;
            student.User.IsAvaliable = null;
            db.Students.Update(student);
            db.Users.Update(student.User);
            db.SaveChanges();
            return RedirectToAction("AddEditStudent", "Students", new { id = student.IdStudent });
        }
        public async Task<IActionResult> ImportStudents()
        {
            bool roleAdmin = UpdateIn(1);

            if (roleAdmin == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/Students/ImportStudents.cshtml", new ImportFileStudents());
        }
        public CheckImport checkModel(AddEditStudent model)
        {
            string regexs_surname = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            string regexs_name = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            CheckImport check = new CheckImport();

            EmployeesController controller = new EmployeesController(db,configuration);
            check.checker = true;

            if (model.SurnameUser == null)
            {
                check.checker = false;
                check.Errors = "Фамилия не может быть пустой";
                return check;
            }
            if (model.NameUser == null)
            {
                check.checker = false;
                check.Errors = "Имя не может быть пустым";
                return check;
            }

            if (model.PatronymicNameUser != null)
            {
                if (model.PatronymicNameUser.StartsWith(" "))
                {
                    check.checker = false;
                    check.Errors = "Отчество указано некорректно";
                    return check;
                }
                else if (model.PatronymicNameUser.EndsWith(" "))
                {
                    check.checker = false;
                    check.Errors = "Отчество указано некорректно";
                    return check;
                }
                else if (model.PatronymicNameUser.Length < 3 || model.PatronymicNameUser.Length > 100)
                {
                    check.checker = false;
                    check.Errors = "Длина отчества указана некорректно";
                    return check;
                }
                else if (!Regex.IsMatch(model.PatronymicNameUser, "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$"))
                {
                    check.checker = false;
                    check.Errors = "Отчество указано некорректно";
                    return check;
                }
            }

            if (model.Email == null)
            {
                check.checker = false;
                check.Errors = "Почта (логин) не может быть пустой";
                return check;
            }
            else if (!controller.IsValidEmail(model.Email))
            {
                check.checker = false;
                check.Errors = "Почта указана некорректно";
                return check;
            }

            if (model.SurnameUser.Length > 100 || model.SurnameUser.Length < 3)
            {
                check.checker = false;
                check.Errors += "Некорректная длина фамилии ";
            }
            if (model.NameUser.Length > 100 || model.NameUser.Length < 3)
            {
                check.checker = false;
                check.Errors += "Некорректная длина имени ";
            }

            if (!Regex.IsMatch(model.NameUser, regexs_name))
            {
                check.checker = false;
                check.Errors += "Имя указано некорректно ";
            }
            if (!Regex.IsMatch(model.SurnameUser, regexs_surname))
            {
                check.checker = false;
                check.Errors += "Фамилия указана некорректно ";
            }

            if (controller.CheckUserEmail(0, model.Email) == false)
            {
                check.checker = false;
                check.Errors += "Указанный Emil существует ";
            }

            if(CheckDateOfBirthdayWithPoint(model.DateOfBirthday) == false)
            {
                check.checker = false;
                check.Errors += "Дата рождения указана некорректно ";
            }
            if (CheckPhoneNumber(0, model.PhoneNumber) == false)
            {
                check.checker = false;
                check.Errors += "Номер телефона указан некорректно/существует ";
            }
            if(model.PathImageStudent == null)
            {
                check.checker = false;
                check.Errors += "Указанная группа не найдена ";
            }
            if (db.Groups.Where(p => p.NameGroup.ToLower().Trim() == model.PathImageStudent!.ToLower().Trim()).Count() <= 0 && model.PathImageStudent != null)
            {
                check.checker = false;
                check.Errors += "Указанная группа не найдена ";
            }
            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportStudents(ImportFileStudents model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedStudents = new List<AddEditStudent>();
            model.passedStudents = new List<AddEditStudent>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditStudent> notPassedStudents = new List<AddEditStudent>();
                List<AddEditStudent> passedStudents = new List<AddEditStudent>();
                List<CheckImport> errorsImport = new List<CheckImport>();
                try
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.First();
                        var rowCount = worksheet.Dimension.Rows;
                        for (var row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var email = worksheet.Cells[row, 1].Value?.ToString();
                                var Surname = worksheet.Cells[row, 2].Value?.ToString();
                                var Name = worksheet.Cells[row, 3].Value?.ToString();
                                var PatronymicName = worksheet.Cells[row, 4].Value?.ToString();
                                var group = worksheet.Cells[row, 5].Value?.ToString();
                                var dateBirthday = worksheet.Cells[row, 6].Value?.ToString();
                                var phoneNumber = worksheet.Cells[row, 7].Value?.ToString();

                                var element = new AddEditStudent()
                                {
                                    Email = email,
                                    SurnameUser = Surname,
                                    NameUser = Name,
                                    PatronymicNameUser = PatronymicName,
                                    PathImageStudent = group,
                                    DateOfBirthday = dateBirthday,
                                    PhoneNumber = phoneNumber
                                };
                                CheckImport check = checkModel(element);

                                if (check.checker == false)
                                {
                                    notPassedStudents.Add(element);
                                    errorsImport.Add(check);
                                }
                                else
                                {
                                    passedStudents.Add(element);

                                    User user = new User();
                                    user.SurnameUser = ToUpperFirstLetter.UpperFirstLetter(element.SurnameUser.Trim());
                                    user.NameUser = ToUpperFirstLetter.UpperFirstLetter(element.NameUser.Trim());
                                    if (element.PatronymicNameUser != null)
                                        user.PatronymicNameUser = ToUpperFirstLetter.UpperFirstLetter(element.PatronymicNameUser.Trim());
                                    else
                                        user.PatronymicNameUser = null;
                                    user.Email = element.Email;
                                    string password = HashPassword.GetRandomPassword(8);
                                    user.Password = HashPassword.hashPassword(password);
                                    user.IsAvaliable = null;
                                    db.Users.Add(user);
                                    db.SaveChanges();
                                    db.UsersRoles.Add(new UsersRole { RoleId = 4, UserId = user.IdUser });
                                    Student student = new Student();
                                    if(element.DateOfBirthday != null)
                                    {
                                        var datebirth = element.DateOfBirthday.Split(".");
                                        student.DateOfBirthday = new DateOnly(Convert.ToInt16(datebirth[2]), Convert.ToInt16(datebirth[1]), Convert.ToInt16(datebirth[0]));
                                    }
                                    student.GroupId = db.Groups.Where(p => p.NameGroup.ToLower().Trim() == element.PathImageStudent.ToLower().Trim()).First().IdGroup;
                                    student.PhoneNumber = element.PhoneNumber;
                                    student.UserId = user.IdUser;
                                    db.Add(student);
                                    db.SaveChanges();
                                    _ = new EmailService(configuration).SendEmailRegistrationAsync(element.Email, password, 1);
                                }
                            }
                            catch
                            {
                                ImportFileStudents models = new ImportFileStudents();
                                models.notPassedStudents = new List<AddEditStudent>();
                                models.passedStudents = new List<AddEditStudent>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedStudents = notPassedStudents;
                                models.passedStudents = passedStudents;
                                models.errorsImport = errorsImport;
                                db.SaveChanges();
                                return View("~/Views/ManageAdmin/Students/ImportStudents.cshtml", models);
                            }
                        }
                    }
                    model.notPassedStudents = notPassedStudents;
                    model.passedStudents = passedStudents;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFileStudents models = new ImportFileStudents();
                    models.notPassedStudents = new List<AddEditStudent>();
                    models.passedStudents = new List<AddEditStudent>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedStudents = notPassedStudents;
                    models.passedStudents = passedStudents;
                    models.errorsImport = errorsImport;
                    db.SaveChanges();
                    return View("~/Views/ManageAdmin/Students/ImportStudents.cshtml", models);
                }
            }
            db.SaveChanges();
            return View("~/Views/ManageAdmin/Students/ImportStudents.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudentPhoneAndDatePost(int id, AddEditStudent model)
        {
            if (UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if(model.ID_User > 0)
            {
                if (ModelState.IsValid)
                {
                    Student student = new Student();
                    student = db.Students.Find(id);
                    student.PhoneNumber = model.PhoneNumber;
                    student.DateOfBirthday = model.DateOfBirthday == null ? null : new DateOnly(Convert.ToInt16(model.DateOfBirthday.Split("-")[0]), Convert.ToInt16(model.DateOfBirthday.Split("-")[1]), Convert.ToInt16(model.DateOfBirthday.Split("-")[2]));
                    db.Students.Update(student);
                    db.SaveChanges();
                    return RedirectToAction("AddEditStudent", "Students", new { id = id });
                }
            }
            return RedirectToAction("AddEditStudent", "Students", new { id = id });
        }
        public async Task<IActionResult> ResumeStudent(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            ResumeModel resumeModel = new ResumeModel();
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Student.IdStudent == id).First();
            if (userDB.Student.Resume == null)
                return RedirectToAction("AddEditStudent", "Students", new { id = id });
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            int Course = (nowDate > augustDate) ? (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation + 1)
                                : (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation);
            resumeModel.course = Course.ToString();
            resumeModel.student_info = userDB.Student;

            if (userDB.Student.Resume != null)
            {
                resumeModel.WorkExperience = userDB.Student.Resume.WorkExperience;
                resumeModel.EducationInfo = userDB.Student.Resume.Education;
                resumeModel.AdditionalInfo = userDB.Student.Resume.AdditionalInformation;
                resumeModel.About = userDB.Student.Resume.AboutStudent;
                resumeModel.Dolzhnost = userDB.Student.Resume.DesiredPosition;
                resumeModel.ProffessionalSkills = userDB.Student.Resume.ProfessionalSkills;
                resumeModel.tags = userDB.Student.Resume.TagsSkills.Split(";");
            }

            return View("~/Views/ManageAdmin/Students/StudentResume.cshtml", resumeModel);
        }
        public IActionResult HideResumeOpen(int value, int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Student.IdStudent == id).First();
            if (userDB.Student.Resume != null)
            {
                userDB.Student.Resume.IsAvaliable = value == 0 ? false : true;
                db.Update(userDB.Student.Resume);
                db.SaveChanges();
            }
            return RedirectToAction("ResumeStudent", new {id = id});
        }
        public IActionResult DeleteResume(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Student.IdStudent == id).First();
            if (userDB.Student.Resume != null)
            {
                if (userDB.Student.Resume.FileWithResume != null)
                {
                    SendFileToServer sendFileTo = new SendFileToServer(configuration);
                    _ = sendFileTo.DeleteOldFile(userDB.Student.Resume.FileWithResume, 1);
                    userDB.Student.Resume.FileWithResume = null;
                    db.Update(userDB.Student.Resume);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("ResumeStudent", new { id = id });
        }

        [HttpPost]
        public IActionResult ResumeUpdate(ResumeModel model, int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Student.IdStudent == id).First();
            if (ModelState.ErrorCount > 1)
            {
                ResumeModel resumeModel = model;

                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                int Course = (nowDate > augustDate) ? (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation + 1)
                                    : (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation);
                resumeModel.course = Course.ToString();
                resumeModel.student_info = userDB.Student;
                return View("~/Views/ManageAdmin/Students/StudentResume.cshtml", resumeModel);
            }
            Resume resume = new Resume();
            if (userDB.Student.Resume != null)
                resume = userDB.Student.Resume;

            resume.Education = model.EducationInfo.Trim();
            resume.DesiredPosition = model.Dolzhnost.Trim();
            resume.AboutStudent = model.About.Trim();
            resume.WorkExperience = model.WorkExperience.Trim();
            resume.ProfessionalSkills = model.ProffessionalSkills.Trim();
            resume.AdditionalInformation = model.AdditionalInfo.Trim();
            resume.TagsSkills = string.Join(";", model.tags);
            resume.Student = userDB.Student;

            if (userDB.Student.Resume != null)
            {
                db.Update(resume);
            }
            else
            {
                resume.IsAvaliable = true;
                db.Add(resume);
            }
            db.SaveChanges();
            return RedirectToAction("ResumeStudent", new { id = id });
        }
    }
}

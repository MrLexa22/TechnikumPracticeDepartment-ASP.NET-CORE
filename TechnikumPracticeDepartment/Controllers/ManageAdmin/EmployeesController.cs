using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using Org.BouncyCastle.Utilities;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.ComponentModel.DataAnnotations;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class EmployeesController : Controller
    {
        public bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public EmployeesController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
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
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/EmployeesTechnikum/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? typeSortListEmployees, int? role, int? activate, string search, int page = 1)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            var listEmployees = db.Users.Include(p => p.UsersRoles).ThenInclude(p => p.Role).ToList();
            listEmployees = listEmployees.Where(p => p.UsersRoles.Where(p => p.RoleId == 1 || p.RoleId == 2).Count() > 0).ToList();

            int pageSize = 15;
            IQueryable<User> Blocks = listEmployees.AsQueryable();
            if (typeSortListEmployees == 1)
            {
                Blocks = Blocks.OrderBy(p => p.SurnameUser+" "+p.NameUser+" "+p.PatronymicNameUser);
            }
            else if (typeSortListEmployees == 2)
            {
                Blocks = Blocks.OrderBy(p => p.Email);
            }

            if(activate == 1)
            {
                Blocks = Blocks.Where(p => p.IsAvaliable == true || p.IsAvaliable == null);
            }
            else if(activate == 2)
            {
                Blocks = Blocks.Where(p => p.IsAvaliable == false);
            }

            if (role != 0 && role != null)
            {
                Blocks = Blocks.Where(p=>p.UsersRoles.Where(p=>p.RoleId == role).Count() > 0);
            }

            if (!String.IsNullOrEmpty(search))
            {
                Blocks = Blocks.Where(p => p.SurnameUser.ToLower().Contains(search.ToLower()) || p.NameUser.ToLower().Contains(search.ToLower()) || p.Email.ToLower().Contains(search.ToLower()));
            }
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexEmployeesModel model = new IndexEmployeesModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Employees(typeSortListEmployees, role, activate, search);
            model.list_employees = items;

            return PartialView("~/Views/ManageAdmin/EmployeesTechnikum/_EmployeesList.cshtml", model);
        }
        public IActionResult downloadExportEmployees(int format, int? role, int? activate)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (format == 1)
            {
                System.IO.MemoryStream content = new System.IO.MemoryStream();
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    var TheListOfObjectsB = db.Users.Include(p => p.UsersRoles).ThenInclude(p => p.Role).
                    Where(p=>(activate == 3 ? (p.IsAvaliable == true || p.IsAvaliable == false || p.IsAvaliable == null) : (activate == 1 ? (p.IsAvaliable == true || p.IsAvaliable == null) : p.IsAvaliable == false) && (role != 0 ? (p.UsersRoles.Where(p=>p.RoleId == role).Count() > 0) : p.UsersRoles.Any()))).
                    Select(a => new ExportModelEmployees() { 
                        Email = a.Email, 
                        SurnameUser = a.SurnameUser,
                        NameUser = a.NameUser,
                        PatronymicNameUser = a.PatronymicNameUser,
                        IsAvaliable = a.IsAvaliable == false ? "-" : "+",
                        Administrator = a.UsersRoles.Where(p=>p.RoleId == 1).Count() > 0 ? "+" : "-",
                        Employee = a.UsersRoles.Where(p => p.RoleId == 2).Count() > 0 ? "+" : "-"
                    }).ToList();
                    csv.WriteRecords(TheListOfObjectsB);
                }
                return File(content.ToArray(), "text/csv", "Export employees " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Сотрудники");
                var TheListOfObjectsB = db.Users.Include(p => p.UsersRoles).ThenInclude(p => p.Role).
                Where(p => (activate == 3 ? (p.IsAvaliable == true || p.IsAvaliable == false || p.IsAvaliable == null) : (activate == 1 ? (p.IsAvaliable == true || p.IsAvaliable == null) : p.IsAvaliable == false) && (role != 0 ? (p.UsersRoles.Where(p => p.RoleId == role).Count() > 0) : p.UsersRoles.Any()))).
                Select(a => new ExportModelEmployees()
                {
                    Email = a.Email,
                    SurnameUser = a.SurnameUser,
                    NameUser = a.NameUser,
                    PatronymicNameUser = a.PatronymicNameUser,
                    IsAvaliable = a.IsAvaliable == false ? "-" : "+",
                    Administrator = a.UsersRoles.Where(p => p.RoleId == 1).Count() > 0 ? "+" : "-",
                    Employee = a.UsersRoles.Where(p => p.RoleId == 2).Count() > 0 ? "+" : "-"
                }).ToList();
                ws.Cells["A1"].LoadFromCollection(TheListOfObjectsB, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export specializations " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public async Task<IActionResult> AddEditEmployee(int id, string? recovery)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            AddEditEmployee model = new AddEditEmployee();
            if(recovery != null)
            {
                if(recovery == "T")
                    ViewData["Recovery"] = "T";
                if (recovery == "T1")
                    ViewData["Recovery"] = "T1";
            }
            if (id != 0)
            {
                var employee = db.Users.Include(p=>p.UsersRoles).Where(p=>p.IdUser == id).First();
                model.Email = employee.Email;
                model.ID_User = employee.IdUser;
                model.SurnameUser = employee.SurnameUser;
                model.NameUser = employee.NameUser;
                model.PatronymicNameUser = employee.PatronymicNameUser;
                model.IsAvaliable = employee.IsAvaliable;
                model.Administrator = employee.UsersRoles.Where(p => p.RoleId == 1).Count() > 0;
                model.Employee = employee.UsersRoles.Where(p => p.RoleId == 2).Count() > 0;
            }
            return View("~/Views/ManageAdmin/EmployeesTechnikum/AddEditEmployee.cshtml", model);
        }
        public bool CheckUserEmail(int ID_User, string Email)
        {
            try
            {
                if (db.Users.Where(p => p.Email.ToLower() == Email.ToLower() && p.IdUser != ID_User).Count() > 0)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool CheckSelectedRoles(bool Administrator, bool Employee)
        {
            if (Administrator == false && Employee == false)
            {
                return false;
            }
            return true;
        }
        public bool CheckPatronymicNameUser(string? PatronymicNameUser)
        {
            if (PatronymicNameUser == null)
            {
                return true;
            }
            else if(PatronymicNameUser.Trim() == "")
            {
                return true;
            }
            if(PatronymicNameUser.StartsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.EndsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.Length < 3 || PatronymicNameUser.Length > 100)
            {
                return false;
            }
            else if(!Regex.IsMatch(PatronymicNameUser, "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$"))
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> AddEditEmployeePost(int id, AddEditEmployee model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    User user = new User();
                    user = db.Users.Find(id);

                    var rolesUser = db.UsersRoles.Where(p => p.UserId == id).ToList();
                    db.UsersRoles.RemoveRange(rolesUser);
                    List<UsersRole> roles = new List<UsersRole>();
                    if (model.Administrator == true)
                        roles.Add(new UsersRole { RoleId = 1, UserId = id });
                    if (model.Employee == true)
                        roles.Add(new UsersRole { RoleId = 2, UserId = id });
                    db.UsersRoles.AddRange(roles);

                    user.SurnameUser = ToUpperFirstLetter.UpperFirstLetter(model.SurnameUser.Trim());
                    user.NameUser = ToUpperFirstLetter.UpperFirstLetter(model.NameUser.Trim());
                    if (model.PatronymicNameUser != null)
                        user.PatronymicNameUser = ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameUser.Trim());
                    else
                        user.PatronymicNameUser = null;
                    user.Email = model.Email;
                    db.Users.Update(user);
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
                db.Users.Add(user);
                db.SaveChanges();

                List<UsersRole> roles = new List<UsersRole>();
                if (model.Administrator == true)
                    roles.Add(new UsersRole { RoleId = 1, UserId = user.IdUser });
                if (model.Employee == true)
                    roles.Add(new UsersRole { RoleId = 2, UserId = user.IdUser });
                db.UsersRoles.AddRange(roles);
                db.SaveChanges();
                _ = new EmailService(configuration).SendEmailRegistrationAsync(model.Email, password, 1);
                return RedirectToAction("AddEditEmployee", "Employees", new { id = user.IdUser, recovery = "T1" });
            }
            return RedirectToAction("AddEditEmployee", "Employees", new { id = id });
        }
        public async Task<IActionResult> ResetPassword(int id)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            User user = new User();
            user = db.Users.Find(id);
            string password = HashPassword.GetRandomPassword(8);
            user.Password = HashPassword.hashPassword(password);
            db.Users.Update(user);
            db.SaveChanges();
            _ = new EmailService(configuration).SendEmailRegistrationAsync(user.Email, password, 2);
            return RedirectToAction("AddEditEmployee", "Employees", new { id = user.IdUser, recovery = "T" });
        }
        public async Task<IActionResult> BlockUnblockUser(int id)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            AddEditEmployee model = new AddEditEmployee();
            if (id != 0)
            {
                var employee = db.Users.Where(p => p.IdUser == id).First();
                if (employee.IsAvaliable == null)
                    employee.IsAvaliable = false;
                else if(employee.IsAvaliable == true)
                    employee.IsAvaliable = false;
                else
                    employee.IsAvaliable = null;
                db.Users.Update(employee);
                db.SaveChanges();
            }
            return RedirectToAction("AddEditEmployee", "Employees", new { id = id });
        }
        public IActionResult ImportEmployees()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/EmployeesTechnikum/ImportEmployees.cshtml", new ImportFileEmployees());
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Сотрудники");

            List<ExportModelEmployees> listExmple = new List<ExportModelEmployees>();
            listExmple.Add(new ExportModelEmployees { 
                Email = "lesha_opeykin@mail.ru",
                SurnameUser = "Опейкин",
                NameUser = "Алексей",
                PatronymicNameUser = "Сергеевич",
                Administrator = "+",
                Employee = "-",
                IsAvaliable = "+"
            });
            listExmple.Add(new ExportModelEmployees
            {
                Email = "ageorgiy8679@rambler.ru",
                SurnameUser = "Аокровский",
                NameUser = "Георгий",
                Administrator = "+",
                Employee = "+",
                IsAvaliable = "+"
            });
            listExmple.Add(new ExportModelEmployees
            {
                Email = "semen2309@gmail.com",
                SurnameUser = "Собчак",
                NameUser = "Семён",
                PatronymicNameUser = "Георгиевич",
                Administrator = "-",
                Employee = "+",
                IsAvaliable = "-"
            });
            listExmple.Add(new ExportModelEmployees
            {
                Email = "lyubov75@mail.ru",
                SurnameUser = "Хренова",
                NameUser = "Любовь",
                Administrator = "-",
                Employee = "+",
                IsAvaliable = "-"
            });

            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file employees.xlsx");
        }
        public CheckImport checkModel(AddEditEmployee model)
        {
            string regexs = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            CheckImport check = new CheckImport();
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
            else if(!IsValidEmail(model.Email))
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

            if (!Regex.IsMatch(model.NameUser, regexs))
            {
                check.checker = false;
                check.Errors += "Имя указано некорректно ";
            }
            if (!Regex.IsMatch(model.SurnameUser, regexs))
            {
                check.checker = false;
                check.Errors += "Фамилия указана некорректно ";
            }

            if (model.Administrator == false && model.Employee == false)
            {
                check.checker = false;
                check.Errors += "Необходимо указать хотя бы одну роль ";
            }

            if (db.Users.Where(p => p.Email.ToLower() == model.Email.ToLower()).Count() > 0)
            {
                check.checker = false;
                check.Errors += "Указанный Emil существует ";
            }

            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportEmployees(ImportFileEmployees model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedEmployees = new List<AddEditEmployee>();
            model.passedEmployees = new List<AddEditEmployee>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditEmployee> notPassedEmployees = new List<AddEditEmployee>();
                List<AddEditEmployee> passedEmployees = new List<AddEditEmployee>();
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
                                var administrator = worksheet.Cells[row, 5].Value?.ToString();
                                var employee = worksheet.Cells[row, 6].Value?.ToString();
                                var isAvaliable = worksheet.Cells[row, 7].Value?.ToString();

                                var element = new AddEditEmployee()
                                {
                                    Email = email,
                                    SurnameUser = Surname,
                                    NameUser = Name,
                                    PatronymicNameUser = PatronymicName,
                                    Administrator = administrator == null ? false : (administrator == "+" ? true : false),
                                    Employee = employee == null ? false : (employee == "+" ? true : false),
                                    IsAvaliable = isAvaliable == null ? false : (isAvaliable == "+" ? true : false)
                                };
                                CheckImport check = checkModel(element);

                                if (check.checker == false)
                                {
                                    notPassedEmployees.Add(element);
                                    errorsImport.Add(check);
                                }
                                else
                                {
                                    passedEmployees.Add(element);

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
                                    user.IsAvaliable = element.IsAvaliable;
                                    db.Users.Add(user);
                                    db.SaveChanges();

                                    List<UsersRole> roles = new List<UsersRole>();
                                    if (element.Administrator == true)
                                        roles.Add(new UsersRole { RoleId = 1, UserId = user.IdUser });
                                    if (element.Employee == true)
                                        roles.Add(new UsersRole { RoleId = 2, UserId = user.IdUser });
                                    db.UsersRoles.AddRange(roles);
                                    db.SaveChanges();
                                    _ = new EmailService(configuration).SendEmailRegistrationAsync(element.Email, password, 1);
                                }
                            }
                            catch
                            {
                                ImportFileEmployees models = new ImportFileEmployees();
                                models.notPassedEmployees = new List<AddEditEmployee>();
                                models.passedEmployees = new List<AddEditEmployee>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedEmployees = notPassedEmployees;
                                models.passedEmployees = passedEmployees;
                                models.errorsImport = errorsImport;
                                db.SaveChanges();
                                return View("~/Views/ManageAdmin/EmployeesTechnikum/ImportEmployees.cshtml", models);
                            }
                        }
                    }
                    model.notPassedEmployees = notPassedEmployees;
                    model.passedEmployees = passedEmployees;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFileEmployees models = new ImportFileEmployees();
                    models.notPassedEmployees = new List<AddEditEmployee>();
                    models.passedEmployees = new List<AddEditEmployee>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedEmployees = notPassedEmployees;
                    models.passedEmployees = passedEmployees;
                    models.errorsImport = errorsImport;
                }
            }
            db.SaveChanges();
            return View("~/Views/ManageAdmin/EmployeesTechnikum/ImportEmployees.cshtml", model);
        }
    }
}

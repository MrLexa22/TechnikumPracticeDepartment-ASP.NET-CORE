using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsOrganizationsPages;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office2010.Excel;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class OrganizationsController : Controller
    {
        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public OrganizationsController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
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
            return View("~/Views/ManageAdmin/Organizations/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? typeSortList, int? filterAvaliable, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var listOrganizations = db.Organizations.ToList();

            int pageSize = 15;
            IQueryable<Organization> Blocks = listOrganizations.AsQueryable();
            if (typeSortList == 1)
                Blocks = Blocks.OrderBy(p => p.NotFullNameOrganization);
            else if (typeSortList == 2)
                Blocks = Blocks.OrderBy(p => p.SurnameContactOfOrganization);

            if (filterAvaliable == 1)
                Blocks = Blocks.Where(p => p.IsAvaliable == true || p.IsAvaliable == null);
            else if (filterAvaliable == 2)
                Blocks = Blocks.Where(p => p.IsAvaliable == false);

            if (!String.IsNullOrEmpty(search))
                Blocks = Blocks.Where(p => p.NotFullNameOrganization.ToLower().Contains(search.ToLower()));

            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexOrganizationModel model = new IndexOrganizationModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Organization(typeSortList, filterAvaliable, search);
            model.list_organization = items;

            return PartialView("~/Views/ManageAdmin/Organizations/_OrganizationsList.cshtml", model);
        }
        public IActionResult downloadExportOrganizations(int format, int? filterAvaliable)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (format == 1)
            {
                System.IO.MemoryStream content = new System.IO.MemoryStream();
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    var TheListOfObjectsB = db.Organizations.
                    Where(p => (filterAvaliable == 3 ? (p.IsAvaliable == true || p.IsAvaliable == false || p.IsAvaliable == null) : (filterAvaliable == 1 ? (p.IsAvaliable == true || p.IsAvaliable == null) : p.IsAvaliable == false))).
                    Select(a => new ExportModelOrganizations()
                    {
                        FullNameOrganization = a.FullNameOrganization,
                        NotFullNameOrganization = a.NotFullNameOrganization,
                        AddressOrganization = a.AddressOrganization,
                        InnOrganization = a.InnOrganization,
                        SurnameContactOfOrganization = a.SurnameContactOfOrganization,
                        NameContactOfOrganization = a.NameContactOfOrganization,
                        PatronymicContactOfOrganization = a.PatronymicContactOfOrganization == null ? "" : a.PatronymicContactOfOrganization,
                        PhoneNumberContactOfOrganization = a.PhoneNumberContactOfOrganization == null ? "" : a.PhoneNumberContactOfOrganization,
                        IsAvaliable = a.IsAvaliable == null ? "1" : a.IsAvaliable == true ? "1" : "0"
                    }).ToList();
                    csv.WriteRecords(TheListOfObjectsB);
                }
                return File(content.ToArray(), "text/csv", "Export organizations " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Организации");
                var TheListOfObjectsB = db.Organizations.
                Where(p => (filterAvaliable == 3 ? (p.IsAvaliable == true || p.IsAvaliable == false || p.IsAvaliable == null) : (filterAvaliable == 1 ? (p.IsAvaliable == true || p.IsAvaliable == null) : p.IsAvaliable == false))).
                Select(a => new ExportModelOrganizations()
                {
                    FullNameOrganization = a.FullNameOrganization,
                    NotFullNameOrganization = a.NotFullNameOrganization,
                    AddressOrganization = a.AddressOrganization,
                    InnOrganization = a.InnOrganization,
                    SurnameContactOfOrganization = a.SurnameContactOfOrganization,
                    NameContactOfOrganization = a.NameContactOfOrganization,
                    PatronymicContactOfOrganization = a.PatronymicContactOfOrganization == null ? "" : a.PatronymicContactOfOrganization,
                    PhoneNumberContactOfOrganization = a.PhoneNumberContactOfOrganization == null ? "" : a.PhoneNumberContactOfOrganization,
                    IsAvaliable = a.IsAvaliable == null ? "1" : a.IsAvaliable == true ? "1" : "0"
                }).ToList();
                ws.Cells["A1"].LoadFromCollection(TheListOfObjectsB, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export organizations " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public bool CheckFullNameOrganization(int ID_Organization, string FullNameOrganization)
        {
            try
            {
                if (db.Organizations.Where(p => p.FullNameOrganization.Trim().ToLower() == FullNameOrganization.ToLower().Trim() && p.IdOrganization != ID_Organization).Count() > 0)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckNotFullNameOrganization(int ID_Organization, string NotFullNameOrganization)
        {
            try
            {
                if (db.Organizations.Where(p => p.NotFullNameOrganization.Trim().ToLower() == NotFullNameOrganization.ToLower().Trim() && p.IdOrganization != ID_Organization).Count() > 0)
                    return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckInnOrganization(int ID_Organization, string INNOrganization)
        {
            try
            {
                if (INNOrganization == null)
                    return true;
                if (Regex.IsMatch(INNOrganization, "^[0-9]{10}(([0-9]{2})?)$"))
                {
                    if (db.Organizations.Where(p => p.InnOrganization == INNOrganization && p.IdOrganization != ID_Organization).Count() > 0)
                        return false;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckPhoneNumber(int ID_Organization, string? PhoneNumber)
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
                    if (db.Organizations.Where(p => p.PhoneNumberContactOfOrganization == PhoneNumber && p.IdOrganization != ID_Organization).Count() > 0)
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
        public async Task<IActionResult> AddEditOrganization(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            AddEditOrganization model = new AddEditOrganization();
            if (id != 0)
            {
                var organizations = db.Organizations.Include(p=>p.Vacancies).Include(p=>p.EmployeeOfOrganizations).ThenInclude(p=>p.User).Where(p => p.IdOrganization == id).First();
                organizations.EmployeeOfOrganizations = organizations.EmployeeOfOrganizations.Where(p => p.User.IsAvaliable == true || p.User.IsAvaliable == null).ToList();
                model.ID_Organization = organizations.IdOrganization;
                model.vacancy_list = organizations.Vacancies.ToList();
                model.FullNameOrganization = organizations.FullNameOrganization;
                model.NotFullNameOrganization = organizations.NotFullNameOrganization;
                model.AddressOrganization = organizations.AddressOrganization;
                model.INNOrganization = organizations.InnOrganization?.ToString();
                model.SurnameUser = organizations.SurnameContactOfOrganization;
                model.NameUser = organizations.NameContactOfOrganization;
                model.PatronymicNameUser = organizations.PatronymicContactOfOrganization?.ToString();
                model.PhoneNumber = organizations.PhoneNumberContactOfOrganization?.ToString();
                model.employeesOrganization = organizations.EmployeeOfOrganizations.ToList();
                model.IsAvaliable = organizations.IsAvaliable;
            }
            else
                model.ID_Organization = 0;
            return View("~/Views/ManageAdmin/Organizations/AddEditOrganization.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditOrganizationPost(int id, AddEditOrganization model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    Organization organization = new Organization();
                    organization = db.Organizations.Find(id);
                    organization.FullNameOrganization = model.FullNameOrganization.Trim();
                    organization.NotFullNameOrganization = model.NotFullNameOrganization.Trim();
                    organization.AddressOrganization = model.AddressOrganization.Trim();
                    organization.InnOrganization = model.INNOrganization?.Trim();
                    organization.SurnameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.SurnameUser.Trim());
                    organization.NameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.NameUser.Trim());
                    if(model.PatronymicNameUser != null)
                        organization.PatronymicContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameUser.Trim());
                    if (model.PhoneNumber != null)
                        organization.PhoneNumberContactOfOrganization = model.PhoneNumber;
                    db.Organizations.Update(organization);
                    db.SaveChanges();
                }
            }
            else
            {
                Organization organization = new Organization();
                organization.FullNameOrganization = model.FullNameOrganization.Trim();
                organization.NotFullNameOrganization = model.NotFullNameOrganization.Trim();
                organization.AddressOrganization = model.AddressOrganization.Trim();
                organization.InnOrganization = model.INNOrganization?.Trim();
                organization.SurnameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.SurnameUser.Trim());
                organization.NameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.NameUser.Trim());
                if (model.PatronymicNameUser != null)
                    organization.PatronymicContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameUser.Trim());
                if (model.PhoneNumber != null)
                    organization.PhoneNumberContactOfOrganization = model.PhoneNumber;
                db.Organizations.Add(organization);
                db.SaveChanges();
                return RedirectToAction("AddEditOrganization", "Organizations", new { id = organization.IdOrganization });
            }
            return RedirectToAction("AddEditOrganization", "Organizations", new { id = id });
        }
        public async Task<IActionResult> AddEditAccountOrganization(int idOrganization, int idUser)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            AddEditOrganizationAccount model = new AddEditOrganizationAccount();
            var organizations = db.Organizations.Include(p => p.EmployeeOfOrganizations).ThenInclude(p => p.User).ToList();
            var organization = organizations.Where(p => p.IdOrganization == idOrganization).First();
            if (idUser != 0)
            {
                model.ID_Organization = idOrganization;
                model.ID_User = idUser;
                var user = db.Users.Where(p => p.IdUser == idUser).First();
                model.SurnameUser = user.SurnameUser;
                model.NameUser = user.NameUser;
                model.Email = user.Email;
                model.PatronymicNameUser = user.PatronymicNameUser;
            }
            else
            {
                model.ID_Organization = idOrganization;
                model.ID_User = 0;
                if (organization.EmployeeOfOrganizations.Where(p => p.User.SurnameUser == organization.SurnameContactOfOrganization && p.User.NameUser == organization.NameContactOfOrganization).Count() > 0)
                    model.IsContact = false;
                else
                    model.IsContact = true;
                model.NameContact = organization.NameContactOfOrganization;
                model.SurnameContact = organization.SurnameContactOfOrganization;
                model.PatronymicnameContact = organization.PatronymicContactOfOrganization;
            }
            return PartialView("~/Views/ManageAdmin/Organizations/_AddEditAccountOrganization.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditAccountOrganizationPost(int idOrganization, int idUser, AddEditOrganizationAccount model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (idUser != 0)
            {
                if (ModelState.IsValid)
                {
                    User user = new User();
                    user = db.Users.Find(idUser);
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
                if (ModelState.IsValid)
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
                    roles.Add(new UsersRole { RoleId = 3, UserId = user.IdUser });
                    db.UsersRoles.AddRange(roles);

                    db.EmployeeOfOrganizations.Add(new EmployeeOfOrganization { OrganizationId = idOrganization, UserId = user.IdUser });
                    db.SaveChanges();

                    if(model.SendEmail == true)
                        _ = new EmailService(configuration).SendEmailRegistrationAsync(model.Email, password, 1);
                }
                return RedirectToAction("AddEditOrganization", "Organizations", new { id = idOrganization });
            }
            return RedirectToAction("AddEditOrganization", "Organizations", new { id = idOrganization });
        }

        [HttpPut]
        public async Task<IActionResult> RecoveryPassword(int idOrganization, int idUser)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (idUser != 0)
            {
                User user = new User();
                user = db.Users.Find(idUser);
                string password = HashPassword.GetRandomPassword(8);
                user.Password = HashPassword.hashPassword(password);
                db.Users.Update(user);
                db.SaveChanges();
                _ = new EmailService(configuration).SendEmailRegistrationAsync(user.Email, password, 2);
            }
            return Json("yes");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccountOrganization(int idOrganization, int idUser)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (idUser != 0)
            {
                User user = new User();
                user = db.Users.Include(p=>p.UsersRoles).Include(p=>p.EmployeeOfOrganization).Where(p=>p.IdUser == idUser).First();
                db.RemoveRange(user.UsersRoles);
                db.RemoveRange(user.EmployeeOfOrganization);
                db.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("AddEditOrganization", "Organizations", new { id = idOrganization });
        }
        public async Task<IActionResult> RemoveOrganization(int idOrganization)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var organization = db.Organizations.Find(idOrganization);
            organization.IsAvaliable = false;
            db.Organizations.Update(organization);
            var employees = db.EmployeeOfOrganizations.Include(p=>p.User).Where(p => p.OrganizationId == idOrganization).ToList();
            foreach(var a in employees)
            {
                a.User.IsAvaliable = false;
                db.Users.Update(a.User);
            }
            db.SaveChanges();
            return RedirectToAction("AddEditOrganization", "Organizations", new { id = idOrganization });
        }
        public async Task<IActionResult> RestoreOrganization(int idOrganization)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var organization = db.Organizations.Find(idOrganization);
            organization.IsAvaliable = null;
            db.Organizations.Update(organization);
            var employees = db.EmployeeOfOrganizations.Include(p => p.User).Where(p => p.OrganizationId == idOrganization).ToList();
            foreach (var a in employees)
            {
                a.User.IsAvaliable = null;
                db.Users.Update(a.User);
            }
            db.SaveChanges();
            return RedirectToAction("AddEditOrganization", "Organizations", new { id = idOrganization });
        }
        public IActionResult ImportOrganizations()
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/Organizations/ImportOrganizations.cshtml", new ImportFileOrganizations());
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Организации");

            List<ExportModelOrganizations> listExmple = new List<ExportModelOrganizations>();
            listExmple.Add(new ExportModelOrganizations
            {
                FullNameOrganization = "Общество с ограниченной ответственностью «Приват Трэйд Инновационные решения»",
                NotFullNameOrganization = "ООО «Приват Трэйд Инновационные решения»",
                AddressOrganization = "212054, г. Москва, ул. Нобеля (Сколково Инновационного Центра Т, д. 7, эт. /помещ. 2/III час.ком./раб.место 57/7",
                InnOrganization = "9731039708",
                SurnameContactOfOrganization = "Агальцов",
                NameContactOfOrganization = "Алексей",
                PatronymicContactOfOrganization = "Николаевич",
                PhoneNumberContactOfOrganization = null, 
                IsAvaliable = "+"
            });
            listExmple.Add(new ExportModelOrganizations
            {
                FullNameOrganization = "Государственное бюджетное общеобразовательное учреждение города Москва «ШКОЛА № 1981»",
                NotFullNameOrganization = "ГБОУ г. Москвы «Школа №1981»",
                AddressOrganization = "117042, город Москва, Бартеневская ул., д.27",
                InnOrganization = "7727236153",
                SurnameContactOfOrganization = "Чермошенцев",
                NameContactOfOrganization = "Александр",
                PatronymicContactOfOrganization = null,
                PhoneNumberContactOfOrganization = "+7(999)815-95-90",
                IsAvaliable = "+"
            });
            listExmple.Add(new ExportModelOrganizations
            {
                FullNameOrganization = "Федеральное государственное бюджетное образовательное учреждение высшего образования \"Российский экономический университет им. Г. В. Плеханова\" Московский приборостроительный техникум, Учебно-производственный тренинговый центр",
                NotFullNameOrganization = "ФГБОУ ВО «РЭУ им. Г.В. Плеханова» МПТ, УПТЦ",
                AddressOrganization = "117149, город Москва, Нахимовский пр-кт, д.21",
                InnOrganization = null,
                SurnameContactOfOrganization = "Шимбирёв",
                NameContactOfOrganization = "Андрей",
                PatronymicContactOfOrganization = "Адреевич",
                PhoneNumberContactOfOrganization = null,
                IsAvaliable = "+"
            });
            listExmple.Add(new ExportModelOrganizations
            {
                FullNameOrganization = "Общество с ограниченной ответственностью \"Газпром Информ\"",
                NotFullNameOrganization = "ООО \"Газпром Информ\"",
                AddressOrganization = "117447, город Москва, Большая Черёмушкинская ул., д. 13 стр. 3",
                InnOrganization = "7727696104",
                SurnameContactOfOrganization = "Бурушкин",
                NameContactOfOrganization = "Алексей",
                PatronymicContactOfOrganization = null,
                PhoneNumberContactOfOrganization = null,
                IsAvaliable = "+"
            });

            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file organizations.xlsx");
        }
        public CheckImport checkModel(AddEditOrganization model)
        {
            string regex_organizationName = "^[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$";
            string regex_address = "^[0-9]{6},[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$";
            string regex_userNames = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            CheckImport check = new CheckImport();
            check.checker = true;

            if (model.FullNameOrganization == null)
            {
                check.checker = false;
                check.Errors = "Полное наименование организации не указано";
                return check;
            }
            if (model.NotFullNameOrganization == null)
            {
                check.checker = false;
                check.Errors = "Сокращённое наименование организации не указано";
                return check;
            }
            if (model.AddressOrganization == null)
            {
                check.checker = false;
                check.Errors = "Адрес организации не указан";
                return check;
            }
            if (model.SurnameUser == null)
            {
                check.checker = false;
                check.Errors = "Фамилия руководителя организации не указана";
                return check;
            }
            if (model.NameUser == null)
            {
                check.checker = false;
                check.Errors = "Имя руководителя организации не указано";
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

            if(model.FullNameOrganization.Length < 10 || model.FullNameOrganization.Length > 254)
            {
                check.checker = false;
                check.Errors += "Длина полного наименования организации некорректна; ";
            }
            if (model.NotFullNameOrganization.Length < 3 || model.FullNameOrganization.Length > 150)
            {
                check.checker = false;
                check.Errors += "Длина сокращённого наименования организации некорректна; ";
            }
            if (model.AddressOrganization.Length < 5 || model.FullNameOrganization.Length > 254)
            {
                check.checker = false;
                check.Errors += "Длина адреса организации некорректна; ";
            }
            if (model.SurnameUser.Length < 3 || model.FullNameOrganization.Length > 100)
            {
                check.checker = false;
                check.Errors += "Длина фамилии руководителя организации некорректна; ";
            }
            if (model.NameUser.Length < 3 || model.NameUser.Length > 100)
            {
                check.checker = false;
                check.Errors += "Длина имени руководителя организации некорректна; ";
            }

            if(CheckFullNameOrganization(0,model.FullNameOrganization) == false)
            {
                check.checker = false;
                check.Errors += "Указанное полное наименование организации уже существует; ";
            }
            if (CheckNotFullNameOrganization(0, model.NotFullNameOrganization) == false)
            {
                check.checker = false;
                check.Errors += "Указанное сокращённое наименование организации уже существует; ";
            }
            if (model.INNOrganization != null)
            {
                if (CheckInnOrganization(0, model.INNOrganization) == false)
                {
                    check.checker = false;
                    check.Errors += "Указанный ИНН существует/указан некорректно; ";
                }
            }
            if(model.PhoneNumber != null)
            {
                if (CheckPhoneNumber(0, model.PhoneNumber) == false)
                {
                    check.checker = false;
                    check.Errors += "Указанный номер телефона существует/указан некорректно; ";
                }
            }

            if(!Regex.IsMatch(model.FullNameOrganization, regex_organizationName))
            {
                check.checker = false;
                check.Errors += "Полное ниаменование организации содержит недопустимые символы (указано некорректно); ";
            }
            if (!Regex.IsMatch(model.NotFullNameOrganization, regex_organizationName))
            {
                check.checker = false;
                check.Errors += "Сокращённое ниаменование организации содержит недопустимые символы (указано некорректно); ";
            }
            if (!Regex.IsMatch(model.AddressOrganization, regex_address))
            {
                check.checker = false;
                check.Errors += "Адрес организации содержит недопустимые символы (указано некорректно); ";
            }
            if (!Regex.IsMatch(model.SurnameUser, regex_userNames))
            {
                check.checker = false;
                check.Errors += "Фамилия руководителя организации содержит недопустимые символы (указано некорректно); ";
            }
            if (!Regex.IsMatch(model.NameUser, regex_userNames))
            {
                check.checker = false;
                check.Errors += "Имя руководителя организации содержит недопустимые символы (указано некорректно); ";
            }

            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportOrganizations(ImportFileOrganizations model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedOrganizations = new List<AddEditOrganization>();
            model.passedOrganizations = new List<AddEditOrganization>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditOrganization> notPassedEmployees = new List<AddEditOrganization>();
                List<AddEditOrganization> passedEmployees = new List<AddEditOrganization>();
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
                                var fullNameOrganization = worksheet.Cells[row, 1].Value?.ToString();
                                var notFullNameOrganization = worksheet.Cells[row, 2].Value?.ToString();
                                var addressOrganization = worksheet.Cells[row, 3].Value?.ToString();
                                var iNNOrganization = worksheet.Cells[row, 4].Value?.ToString();
                                var surnameUser = worksheet.Cells[row, 5].Value?.ToString();
                                var nameUser = worksheet.Cells[row, 6].Value?.ToString();
                                var patronymicNameUser = worksheet.Cells[row, 7].Value?.ToString();
                                var phoneNumber = worksheet.Cells[row, 8].Value?.ToString();
                                var isAvaliable = worksheet.Cells[row, 9].Value?.ToString();

                                var element = new AddEditOrganization()
                                {
                                    FullNameOrganization = fullNameOrganization,
                                    NotFullNameOrganization = notFullNameOrganization,
                                    AddressOrganization = addressOrganization,
                                    INNOrganization = iNNOrganization,
                                    SurnameUser = surnameUser,
                                    NameUser = nameUser,
                                    PatronymicNameUser = patronymicNameUser,
                                    PhoneNumber = phoneNumber,
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
                                    Organization organization = new Organization();
                                    organization.FullNameOrganization = element.FullNameOrganization.Trim();
                                    organization.NotFullNameOrganization = element.NotFullNameOrganization.Trim();
                                    organization.AddressOrganization = element.AddressOrganization.Trim();
                                    organization.InnOrganization = element.INNOrganization?.Trim();
                                    organization.SurnameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(element.SurnameUser.Trim());
                                    organization.NameContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(element.NameUser.Trim());
                                    if (element.PatronymicNameUser != null)
                                        organization.PatronymicContactOfOrganization = ToUpperFirstLetter.UpperFirstLetter(element.PatronymicNameUser.Trim());
                                    if (element.PhoneNumber != null)
                                        organization.PhoneNumberContactOfOrganization = element.PhoneNumber;
                                    organization.IsAvaliable = element.IsAvaliable;
                                    db.Organizations.Add(organization);
                                    db.SaveChanges();
                                }
                            }
                            catch
                            {
                                ImportFileOrganizations models = new ImportFileOrganizations();
                                models.notPassedOrganizations = new List<AddEditOrganization>();
                                models.passedOrganizations = new List<AddEditOrganization>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedOrganizations = notPassedEmployees;
                                models.passedOrganizations = passedEmployees;
                                models.errorsImport = errorsImport;
                                db.SaveChanges();
                                return View("~/Views/ManageAdmin/Organizations/ImportOrganizations.cshtml", models);
                            }
                        }
                    }
                    model.notPassedOrganizations = notPassedEmployees;
                    model.passedOrganizations = passedEmployees;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFileOrganizations models = new ImportFileOrganizations();
                    models.notPassedOrganizations = new List<AddEditOrganization>();
                    models.passedOrganizations = new List<AddEditOrganization>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedOrganizations = notPassedEmployees;
                    models.passedOrganizations = passedEmployees;
                    models.errorsImport = errorsImport;
                    return View("~/Views/ManageAdmin/Organizations/ImportOrganizations.cshtml", models);
                }
            }
            db.SaveChanges();
            return View("~/Views/ManageAdmin/Organizations/ImportOrganizations.cshtml", model);
        }
    }
}

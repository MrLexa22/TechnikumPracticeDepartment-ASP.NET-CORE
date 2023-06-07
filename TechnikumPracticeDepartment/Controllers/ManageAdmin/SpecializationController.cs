using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Excel.EPPlus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.ModelsDB;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Group = TechnikumPracticeDepartment.ModelsDB.Group;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class SpecializationController : Controller
    {
        TechnikumPracticeDepartmentContext db;
        public SpecializationController(TechnikumPracticeDepartmentContext context)
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
            if (role > 0 && rolesUser.Where(p=>p.RoleId == role).Count() > 0)
                return true;
            else
                return false;
        }
        public IActionResult Index()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/Specialization/Index.cshtml");
        }
        public async Task<IActionResult> GetList(int? typeSortListSpecialization, string search, int page = 1)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            List<Specialization> listSpecializations = new List<Specialization>();
            if(nowDate > augustDate)
                listSpecializations = db.Specializations.Include(p => p.Groups.Where(p => p.YearOfGraduation > yearNow).OrderBy(p=>p.YearStartEducation).ThenBy(p => p.NameGroup)).ToList();
            else
                listSpecializations = db.Specializations.Include(p => p.Groups.Where(p => p.YearOfGraduation >= yearNow).OrderBy(p => p.YearStartEducation).ThenBy(p => p.NameGroup)).ToList();

            int pageSize = 15;
            IQueryable<Specialization> Blocks = listSpecializations.AsQueryable();
            if (typeSortListSpecialization == 1)
            {
                Blocks = Blocks.OrderBy(p=>p.SpecializationName);
            }
            if (typeSortListSpecialization == 2)
            {
                Blocks = Blocks.OrderBy(p => p.SpecializationCode);
            }
            if (!String.IsNullOrEmpty(search))
            {
                Blocks = Blocks.Where(p => p.SpecializationName.ToLower().Contains(search.ToLower()) || p.SpecializationCode.ToLower().Contains(search.ToLower()) || p.NameQualification.ToLower().Contains(search.ToLower()));
            }
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexSpecializationModel model = new IndexSpecializationModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Specialization(typeSortListSpecialization, search);
            model.list_specialnosti = items;

            return PartialView("~/Views/ManageAdmin/Specialization/_SpecialnizationList.cshtml", model);
        }
        public async Task<IActionResult> AddEditSpecialization(int id)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            AddEditSpecializationModel model = new AddEditSpecializationModel();
            if(id != 0)
            {
                var specialization = db.Specializations.Find(id);
                model.SpecializationName = specialization.SpecializationName;
                model.ID_Specialization = specialization.IdSpecialization;
                model.SpecizalizationQualif = specialization.NameQualification;
                model.SpecializationCode = specialization.SpecializationCode;

                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                List<Group> Groups = new List<Group>();
                if (nowDate > augustDate)
                    Groups = db.Groups.Include(p => p.Specialization).Where(p => p.YearOfGraduation > yearNow).ToList();
                else
                    Groups = db.Groups.Include(p => p.Specialization).Where(p => p.YearOfGraduation >= yearNow).ToList();

                model.list_groups = new List<GroupsSpecialization>();
                foreach(var a in Groups)
                {
                    GroupsSpecialization group = new GroupsSpecialization();
                    group.ID_Group = a.IdGroup;
                    group.NameGroup = a.NameGroup;
                    group.SpecName = a.Specialization.SpecializationCode;
                    group.SpecID = a.SpecializationId;
                    if (a.SpecializationId == id)
                        group.Selected = true;
                    else
                        group.Selected = false;
                    model.list_groups.Add(group);
                }
                model.list_groups = model.list_groups.OrderByDescending(p => p.Selected).ThenBy(p => p.SpecID).ThenBy(p=>p.NameGroup).ToList();
            }
            return View("~/Views/ManageAdmin/Specialization/AddEditSpecialization.cshtml",model);
        }
        public bool CheckSpecializationCode(int ID_Specialization, string SpecializationCode)
        {
            if (db.Specializations.Where(p => p.SpecializationCode.ToLower() == SpecializationCode.ToLower() && p.IdSpecialization != ID_Specialization).Count() > 0)
            {
                return false;
            }
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> AddEditSpecializationPost(int id, AddEditSpecializationModel model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    Specialization specialization = new Specialization();
                    specialization = db.Specializations.Find(id);
                    specialization.SpecializationName = ToUpperFirstLetter.UpperFirstLetter(model.SpecializationName.Trim());
                    specialization.NameQualification = ToUpperFirstLetter.UpperFirstLetter(model.SpecizalizationQualif.Trim());
                    specialization.SpecializationCode = model.SpecializationCode.Trim();
                    db.Specializations.Update(specialization);
                    db.SaveChanges();
                }
            }
            else
            {
                Specialization specialization = new Specialization();
                specialization.SpecializationName = ToUpperFirstLetter.UpperFirstLetter(model.SpecializationName.Trim());
                specialization.NameQualification = ToUpperFirstLetter.UpperFirstLetter(model.SpecizalizationQualif.Trim());
                specialization.SpecializationCode = model.SpecializationCode.Trim();
                db.Specializations.Add(specialization);
                db.SaveChanges();
                id = specialization.IdSpecialization;
            }

            return RedirectToAction("AddEditSpecialization", "Specialization", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> EditGroupsSpecialization(int id, AddEditSpecializationModel model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (model.list_groups == null)
                return RedirectToAction("AddEditSpecialization", "Specialization", new { id = id });

            var groups = model.list_groups;
            var groups_db = db.Groups.ToList();
            var frist_spec = db.Specializations.Where(p => p.IdSpecialization != id).OrderBy(p => p.SpecializationName).ToList();
            if (frist_spec.Count <= 0)
                frist_spec = db.Specializations.ToList();
            var first_specialnost = frist_spec.First().IdSpecialization;

            foreach (var a in groups)
            {
                if (a.Selected == true)
                {
                    if (a.SpecID == groups_db.Where(p => p.IdGroup == a.ID_Group).First().SpecializationId && a.SpecID == id)
                    { }
                    else
                    {
                        Group group = groups_db.Where(p => p.IdGroup == a.ID_Group).First();
                        group.SpecializationId = id;
                        db.Groups.Update(group);
                    }
                }
                else
                {
                    if (a.SpecID == groups_db.Where(p => p.IdGroup == a.ID_Group).First().SpecializationId && a.SpecID != id)
                    { }
                    else
                    {
                        Group group = groups_db.Where(p => p.IdGroup == a.ID_Group).First();
                        group.SpecializationId = first_specialnost;
                        db.Groups.Update(group);
                    }
                }
            }
            db.SaveChanges();
            return RedirectToAction("AddEditSpecialization", "Specialization", new { id = id });
        }
        public IActionResult downloadExportSpecialization(int format)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            if (format == 1)
            {
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    var TheListOfObjectsB = db.Specializations.Select(a => new ExportModelSpecialization() { CodeSpecialization = a.SpecializationCode, NameSpecialization = a.SpecializationName, NameQualificationSpecialization = a.NameQualification }).ToList();
                    csv.WriteRecords(TheListOfObjectsB);
                }
                return File(content.ToArray(), "text/csv", "Export specializations " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Специальности");
                var TheListOfObjectsB = db.Specializations.Select(a => new ExportModelSpecialization() { CodeSpecialization = a.SpecializationCode, NameSpecialization = a.SpecializationName, NameQualificationSpecialization = a.NameQualification }).ToList();
                ws.Cells["A1"].LoadFromCollection(TheListOfObjectsB, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export specializations " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public IActionResult ImportSpecialization()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/Specialization/ImportSpecialization.cshtml", new ImportFileSpecialization());
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Специальности");
            List<ExportModelSpecialization> listExmple = new List<ExportModelSpecialization>();
            ExportModelSpecialization model = new ExportModelSpecialization();
            model.CodeSpecialization = "09.02.07-П";
            model.NameSpecialization = "Информационные системы и программирование";
            model.NameQualificationSpecialization = "Программист";
            listExmple.Add(model);
            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file specializations.xlsx");
        }
        public CheckImport checkModel(AddEditSpecializationModel model)
        {
            string regex_codespecialization = "[0-9]{2}.[0-9]{2}.[0-9]{2}(-[А-Я][А-Я]?)?";
            string regex_namespecialization = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            string regex_namequalification = "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$";
            CheckImport check = new CheckImport();
            check.checker = true;
            if (model.SpecializationCode == null)
            {
                check.checker = false;
                check.Errors = "Код специальности не может быть пустым";
                return check;
            }
            if (model.SpecializationName == null)
            {
                check.checker = false;
                check.Errors = "Наименование специальности не может быть пустым";
                return check;
            }
            if (model.SpecizalizationQualif == null)
            {
                check.checker = false;
                check.Errors = "Наименование квалификации не может быть пустым";
                return check;
            }
        
            if(model.SpecializationName.Length > 100 || model.SpecializationName.Length < 5)
            {
                check.checker = false;
                check.Errors += "Некорректная длина наименование специальности ";
            }
            if (model.SpecizalizationQualif.Length > 50 || model.SpecizalizationQualif.Length < 4)
            {
                check.checker = false;
                check.Errors += "Некорректная длина квалификации ";
            }

            if(!Regex.IsMatch(model.SpecializationCode, regex_codespecialization))
            {
                check.checker = false;
                check.Errors += "Код специальности указан некорректно ";
            }
            if (!Regex.IsMatch(model.SpecializationName, regex_namespecialization))
            {
                check.checker = false;
                check.Errors += "Наименование специальности указано некорректно ";
            }
            if (!Regex.IsMatch(model.SpecizalizationQualif, regex_namequalification))
            {
                check.checker = false;
                check.Errors += "Наименование квалификации указано некорректно ";
            }
        
            if(db.Specializations.Where(p => p.SpecializationCode.ToLower() == model.SpecializationCode.ToLower()).Count() > 0)
            {
                check.checker = false;
                check.Errors += "Специальность с таким кодом уже существует";
            }
            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportSpecialization(ImportFileSpecialization model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedSpecializations = new List<AddEditSpecializationModel>();
            model.passedSpecializations = new List<AddEditSpecializationModel>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditSpecializationModel> notPassedSpecializations = new List<AddEditSpecializationModel>();
                List<AddEditSpecializationModel> passedSpecializations = new List<AddEditSpecializationModel>();
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
                                var codeSpecialization = worksheet.Cells[row, 1].Value?.ToString();
                                var nameSpecialization = worksheet.Cells[row, 2].Value?.ToString();
                                var nameQualificationSpecialization = worksheet.Cells[row, 3].Value?.ToString();
                                var element = new AddEditSpecializationModel()
                                {
                                    SpecializationCode = codeSpecialization,
                                    SpecializationName = nameSpecialization,
                                    SpecizalizationQualif = nameQualificationSpecialization
                                };
                                CheckImport check = checkModel(element);
                                if (check.checker == false)
                                {
                                    notPassedSpecializations.Add(element);
                                    errorsImport.Add(check);
                                }
                                else
                                {
                                    passedSpecializations.Add(element);
                                    Specialization specialization = new Specialization();
                                    specialization.SpecializationCode = element.SpecializationCode;
                                    specialization.SpecializationName = ToUpperFirstLetter.UpperFirstLetter(element.SpecializationName);
                                    specialization.NameQualification = ToUpperFirstLetter.UpperFirstLetter(element.SpecizalizationQualif);
                                    db.Specializations.Add(specialization);
                                }
                            }
                            catch
                            {
                                ImportFileSpecialization models = new ImportFileSpecialization();
                                models.notPassedSpecializations = new List<AddEditSpecializationModel>();
                                models.passedSpecializations = new List<AddEditSpecializationModel>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedSpecializations = notPassedSpecializations;
                                models.passedSpecializations = passedSpecializations;
                                models.errorsImport = errorsImport;
                                return View("~/Views/ManageAdmin/Specialization/ImportSpecialization.cshtml", models);
                                db.SaveChanges();
                            }
                        }
                    }
                    model.notPassedSpecializations = notPassedSpecializations;
                    model.passedSpecializations = passedSpecializations;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFileSpecialization models = new ImportFileSpecialization();
                    models.notPassedSpecializations = new List<AddEditSpecializationModel>();
                    models.passedSpecializations = new List<AddEditSpecializationModel>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedSpecializations = notPassedSpecializations;
                    models.passedSpecializations = passedSpecializations;
                    models.errorsImport = errorsImport;
                    db.SaveChanges();
                }
            }
            db.SaveChanges();
            return View("~/Views/ManageAdmin/Specialization/ImportSpecialization.cshtml", model);
        }
    }
}

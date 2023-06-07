using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsPracticePages;
using NuGet.Packaging;
using System.ComponentModel.DataAnnotations;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.Controllers.ManagePractice
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class PracticeController : Controller
    {
        TechnikumPracticeDepartmentContext db;
        public PracticeController(TechnikumPracticeDepartmentContext context)
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
            return View("~/Views/ManagePractice/Practices/Index.cshtml", db.Specializations.ToList());
        }
        public async Task<IActionResult> GetList(int? typeSortList, int? filterListBySpecializaion, string search, int page = 1)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            var listPractice = db.Practices.Include(p => p.PracticeSpecializations).ThenInclude(p => p.Specialization).ToList();

            int pageSize = 15;
            IQueryable<Practice> Blocks = listPractice.AsQueryable();
            if (typeSortList == 1)
            {
                Blocks = Blocks.OrderBy(p => p.NamePractice);
            }
            else if (typeSortList == 2)
            {
                Blocks = Blocks.OrderBy(p => p.NameProfModule);
            }

            if (filterListBySpecializaion != 0 && filterListBySpecializaion != null)
            {
                Blocks = Blocks.Where(p => p.PracticeSpecializations.Where(p => p.SpecializationId == filterListBySpecializaion).Count() > 0);
            }

            if (!String.IsNullOrEmpty(search))
            {
                Blocks = Blocks.Where(p => p.NamePractice.ToLower().Contains(search.ToLower()) || p.NameProfModule.ToLower().Contains(search.ToLower()));
            }
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexPracticeModel model = new IndexPracticeModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Practice(typeSortList, filterListBySpecializaion, search);
            model.list_practice = items;

            return PartialView("~/Views/ManagePractice/Practices/_PracticesList.cshtml", model);
        }
        public async Task<IActionResult> AddEditPractice(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            AddEditPractice model = new AddEditPractice();
            model.IsSaved = true;

            var prof_modules = db.Practices.Select(p => p.NameProfModule).ToList();
            prof_modules = prof_modules.Distinct().ToList();
            model.old_profModules = prof_modules;
            
            if (id != 0)
            {
                var practice = db.Practices.Include(p=>p.PracticeSpecializations).ThenInclude(p=>p.Specialization).Where(p => p.IdPractice == id).First();
                var specializaions = db.Specializations.ToList();
                var selected_spec = practice.PracticeSpecializations;

                List<Specialization> selected_speciazliaions = practice.PracticeSpecializations.Select(p => p.Specialization).ToList();
                var not_selectedSpecilizaions = specializaions.Except(selected_speciazliaions);

                List<SpecializaionWithBool> list_speciliazaion = new List<SpecializaionWithBool>();
                list_speciliazaion.AddRange(selected_speciazliaions.Select(p => new SpecializaionWithBool
                {
                    ID_Specializaion = p.IdSpecialization,
                    NameSpecializaion = p.SpecializationName,
                    CodeSpecializaion = p.SpecializationCode,
                    IsSelected = true
                }));
                list_speciliazaion.AddRange(not_selectedSpecilizaions.Select(p => new SpecializaionWithBool
                {
                    ID_Specializaion = p.IdSpecialization,
                    NameSpecializaion = p.SpecializationName,
                    CodeSpecializaion = p.SpecializationCode,
                    IsSelected = false
                }));

                model.ID_Practice = practice.IdPractice;
                model.NamePractice = practice.NamePractice;
                model.NameProfModuel = practice.NameProfModule;
                model.list_specializaion = list_speciliazaion;

                if (db.PracticeCharts.Where(p => p.PracticeId == id).Count() <= 0)
                    model.IsAvaliableForDelete = true;
                else
                    model.IsAvaliableForDelete = false;
            }
            else
            {
                model.ID_Practice = 0;
                var specializaions = db.Specializations.ToList();
                List<SpecializaionWithBool> list_speciliazaion = new List<SpecializaionWithBool>();
                list_speciliazaion.AddRange(specializaions.Select(p => new SpecializaionWithBool
                {
                    ID_Specializaion = p.IdSpecialization,
                    NameSpecializaion = p.SpecializationName,
                    CodeSpecializaion = p.SpecializationCode,
                    IsSelected = false
                }));
                model.list_specializaion = list_speciliazaion;
                model.IsAvaliableForDelete = false;
            }
            return View("~/Views/ManagePractice/Practices/AddEditPractice.cshtml", model);
        }
        public bool CheckNamePractice(int ID_Practice, string NameProfModuel, string NamePractice)
        {
           var namePractice = NamePractice.ToLower().Replace("«", "").Replace("»", "").Replace("\"", "");
            if (db.Practices.Where(p => p.NamePractice.ToLower().Replace("«","").Replace("»","").Replace("\"","") == namePractice && p.IdPractice != ID_Practice).Count() > 0)
                return false;
            if (!(NameProfModuel[0] + NameProfModuel[1] + NameProfModuel[2] == NamePractice[0] + NamePractice[1] + NamePractice[2] && NameProfModuel[0] == 'П' && NameProfModuel[1] == 'Д' && NameProfModuel[3] == 'Д'))
            {
                if (NameProfModuel[3] + NameProfModuel[4] != NamePractice[3] + NamePractice[4])
                    return false;
            }
            return true;
        }
        public bool CheckSelecteSpecializaions(List<SpecializaionWithBool> list_specializaion)
        {
            if (list_specializaion.Where(p => p.IsSelected == true).Count() <= 0)
                return false;
            return true;
        }

        [HttpPost]
        public async Task<IActionResult> AddEditPracticePost(int id, AddEditPractice model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if(model.list_specializaion.Where(p=>p.IsSelected == true).Count() <= 0)
            {
                var prof_modules = db.Practices.Select(p => p.NameProfModule).ToList();
                prof_modules = prof_modules.Distinct().ToList();
                model.old_profModules = prof_modules;
                model.IsSaved = false;
                if (id > 0)
                {
                    if (db.PracticeCharts.Where(p => p.PracticeId == id).Count() <= 0)
                        model.IsAvaliableForDelete = true;
                    else
                        model.IsAvaliableForDelete = false;
                }
                ModelState.AddModelError("list_specializaion", "Выберите хотя бы одну специальность");
                return View("~/Views/ManagePractice/Practices/AddEditPractice.cshtml", model);
            }

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    Practice practice = new Practice();
                    practice = db.Practices.Find(id);

                    practice.NameProfModule = model.NameProfModuel.Trim().Replace("«", "\"").Replace("»", "\"");
                    practice.NamePractice = model.NamePractice.Trim().Replace("«", "\"").Replace("»", "\"");
                    db.Practices.Update(practice);

                    var old_selectedSpecializaions = db.PracticeSpecializations.Where(p => p.PracticeId == id).ToList();
                    db.PracticeSpecializations.RemoveRange(old_selectedSpecializaions);

                    var add_selectedSepecializaion = model.list_specializaion.Where(p => p.IsSelected == true).ToList();
                    foreach(var a in add_selectedSepecializaion)
                        db.PracticeSpecializations.Add(new PracticeSpecialization { PracticeId = id, SpecializationId = a.ID_Specializaion});

                    db.SaveChanges();
                }
            }
            else
            {
                Practice practice = new Practice();
                practice.NameProfModule = model.NameProfModuel.Trim().Replace("«", "\"").Replace("»", "\"");
                practice.NamePractice = model.NamePractice.Trim().Replace("«", "\"").Replace("»", "\"");
                db.Practices.Add(practice);
                db.SaveChanges();

                var add_selectedSepecializaion = model.list_specializaion.Where(p => p.IsSelected == true).ToList();
                foreach (var a in add_selectedSepecializaion)
                    db.PracticeSpecializations.Add(new PracticeSpecialization { PracticeId = practice.IdPractice, SpecializationId = a.ID_Specializaion });

                db.SaveChanges();
                id = practice.IdPractice;
            }
            return RedirectToAction("AddEditPractice", "Practice", new { id = id });
        }
        public async Task<IActionResult> DeletePractice(int id)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            var practice = db.Practices.Find(id);
            if (practice == null)
                return RedirectToAction("Index", "Practice");
            else if(db.PracticeCharts.Where(p=>p.PracticeId == id).Count() > 0)
                return RedirectToAction("Index", "Practice");
            else
            {
                db.Practices.Remove(practice);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Practice");
        }
        public IActionResult downloadExportPractices(int format, int? filterListBySpecializaion)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            if (format == 1)
            {
                System.IO.MemoryStream content = new System.IO.MemoryStream();
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    var TheListOfObjectsB = db.Practices.Include(p => p.PracticeSpecializations).ThenInclude(p => p.Specialization).
                    Where(p => filterListBySpecializaion == 0 ? p.PracticeSpecializations.Count() > -1 : p.PracticeSpecializations.Where(p => p.SpecializationId == filterListBySpecializaion).Count() > 0).
                    Select(a => new ExportModelPractice()
                    {
                        NameProfModule = a.NameProfModule,
                        NamePractice = a.NamePractice,
                        Specializaions = string.Join("| ", a.PracticeSpecializations.Select(p => p.Specialization.SpecializationCode)),
                    }).ToList();
                    csv.WriteRecords(TheListOfObjectsB);
                }
                return File(content.ToArray(), "text/csv", "Export practices " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Практики");
                var TheListOfObjectsB = db.Practices.Include(p => p.PracticeSpecializations).ThenInclude(p => p.Specialization).
                Where(p => filterListBySpecializaion == 0 ? p.PracticeSpecializations.Count() > -1 : p.PracticeSpecializations.Where(p => p.SpecializationId == filterListBySpecializaion).Count() > 0).
                Select(a => new ExportModelPractice()
                {
                    NameProfModule = a.NameProfModule,
                    NamePractice = a.NamePractice,
                    Specializaions = string.Join("; ", a.PracticeSpecializations.Select(p => p.Specialization.SpecializationCode)),
                }).ToList();
                ws.Cells["A1"].LoadFromCollection(TheListOfObjectsB, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export practices " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public IActionResult ImportPractices()
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManagePractice/Practices/ImportPractices.cshtml", new ImportFilePractice());
        }
        public CheckImport checkModel(AddEditPractice model)
        {
            string regex_nameProfModule = "^(ПМ.[0-9]{2}|ПДП.) (\"|«)[А-Яа-яЁё,. ()]{5,}(\"|»)$";
            string regex_namePractice = "^(ПП.[0-9]{2}.[0-9]{2}|ПДП.) (\"|«)[А-Яа-яЁё,. ()]{5,}(\"|»)$";
            CheckImport check = new CheckImport();
            check.checker = true;

            if(model.NameProfModuel == null)
            {
                check.checker = false;
                check.Errors = "Наименование проф. модуля не может быть пустым";
                return check;
            }
            if (model.NamePractice == null)
            {
                check.checker = false;
                check.Errors = "Наименование практики не может быть пустым";
                return check;
            }
            if (model.list_specializaion == null)
            {
                check.checker = false;
                check.Errors = "Необходимо указать минимум 1 специальности";
                return check;
            }

            if(!Regex.IsMatch(model.NamePractice, regex_namePractice))
            {
                check.checker = false;
                check.Errors += "Некорректное наименовани практики Пример: ПП.11.01 \"(«)Название, название. Название\"(») ИЛИ ПДП. \"(«)Название, название. Название\"(») ";
            }
            if (!Regex.IsMatch(model.NameProfModuel, regex_nameProfModule))
            {
                check.checker = false;
                check.Errors += "Некорректное наименовани проф. модуля. Пример: ПМ.11 \"(«)Название, название. Название\"(») ИЛИ ПДП \"(«)Название, название. Название\"(») ";
            }

            if(model.NamePractice.Length < 5 || model.NamePractice.Length > 250)
            {
                check.checker = false;
                check.Errors += "Длина наименования практики указана некорректно ";
            }
            if (model.NameProfModuel.Length < 5 || model.NameProfModuel.Length > 250)
            {
                check.checker = false;
                check.Errors += "Длина наименования проф. модуля указана некорректно ";
            }

            if(!CheckNamePractice(0,model.NameProfModuel,model.NamePractice))
            {
                check.checker = false;
                check.Errors += "Такая практика с таким названием уже существует (ПП.XX, ПМ.XX: XX не равны) ";
            }

            try
            {
                foreach(var a in model.list_specializaion)
                {
                    if(a.CodeSpecializaion != null && a.CodeSpecializaion?.Trim() != "")
                    {
                        if(db.Specializations.Where(p=>p.SpecializationCode.ToLower() == a.CodeSpecializaion.ToLower().Trim()).Count() <= 0)
                        {
                            check.checker = false;
                            check.Errors += "Специальность с кодом "+a.CodeSpecializaion+" не найдена ";
                        }
                    }
                    else
                    {
                        check.checker = false;
                        check.Errors += "Специальность не может быть пустой, возможно есть ; в последней специальности из списка ";
                    }
                }
            }
            catch
            {
                check.checker = false;
                check.Errors += "Ошибка в столбце со специальностями ";
            }

            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportPractice(ImportFilePractice model)
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedPractices = new List<AddEditPractice>();
            model.passedPractices = new List<AddEditPractice>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditPractice> notPassedPractices = new List<AddEditPractice>();
                List<AddEditPractice> passedPractices = new List<AddEditPractice>();
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
                                var nameProfModule = worksheet.Cells[row, 1].Value?.ToString();
                                var namePractice = worksheet.Cells[row, 2].Value?.ToString();
                                var specializaions = worksheet.Cells[row, 3].Value?.ToString();
                                List<SpecializaionWithBool> list_spec = new List<SpecializaionWithBool>();
                                var listCodes = specializaions.Split(";");
                                foreach(var a in listCodes)
                                {
                                    list_spec.Add(new SpecializaionWithBool { CodeSpecializaion = a });
                                }
                                var element = new AddEditPractice()
                                {
                                    NameProfModuel = nameProfModule,
                                    NamePractice = namePractice,
                                    list_specializaion = list_spec,
                                };
                                CheckImport check = checkModel(element);

                                if (check.checker == false)
                                {
                                    notPassedPractices.Add(element);
                                    errorsImport.Add(check);
                                }
                                else
                                {
                                    passedPractices.Add(element);

                                    Practice practice = new Practice();
                                    practice.NameProfModule = element.NameProfModuel.Trim().Replace("«", "\"").Replace("»", "\"");
                                    practice.NamePractice = element.NamePractice.Trim().Replace("«", "\"").Replace("»", "\"");
                                    db.Practices.Add(practice);
                                    db.SaveChanges();

                                    List<Specialization> slected_Specializaions = new List<Specialization>();
                                    foreach (var a in list_spec)
                                    {
                                        var spec = db.Specializations.Where(p => p.SpecializationCode.ToLower() == a.CodeSpecializaion.ToLower().Trim()).First();
                                        slected_Specializaions.Add(spec);
                                    }

                                    foreach (var a in slected_Specializaions)
                                        db.PracticeSpecializations.Add(new PracticeSpecialization { PracticeId = practice.IdPractice, SpecializationId = a.IdSpecialization });
                                    db.SaveChanges();
                                }
                            }
                            catch
                            {
                                ImportFilePractice models = new ImportFilePractice();
                                models.notPassedPractices = new List<AddEditPractice>();
                                models.passedPractices = new List<AddEditPractice>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedPractices = notPassedPractices;
                                models.passedPractices = passedPractices;
                                models.errorsImport = errorsImport;
                                db.SaveChanges();
                                return View("~/Views/ManagePractice/Practices/ImportPractices.cshtml", models);
                            }
                        }
                    }
                    model.notPassedPractices = notPassedPractices;
                    model.passedPractices = passedPractices;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFilePractice models = new ImportFilePractice();
                    models.notPassedPractices = new List<AddEditPractice>();
                    models.passedPractices = new List<AddEditPractice>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedPractices = notPassedPractices;
                    models.passedPractices = passedPractices;
                    models.errorsImport = errorsImport;
                    db.SaveChanges();
                    return View("~/Views/ManagePractice/Practices/ImportPractices.cshtml", models);
                }
            }
            db.SaveChanges();
            return View("~/Views/ManagePractice/Practices/ImportPractices.cshtml", model);
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false && UpdateIn(2) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Практики");

            List<ExportModelPractice> listExmple = new List<ExportModelPractice>();
            listExmple.Add(new ExportModelPractice
            {
                NameProfModule = "ПМ.01 \"Профессиональный модуль\"",
                NamePractice = "ПП.01.01 \"Практика по профессиональному модулю\"",
                Specializaions = "09.02.07-П; 09.02.01"
            });
            listExmple.Add(new ExportModelPractice
            {
                NameProfModule = "ПМ.12 \"Профессиональный модуль\"",
                NamePractice = "ПП.12.01 \"Практика по профессиональному модулю\"",
                Specializaions = "09.02.07-П"
            });
            listExmple.Add(new ExportModelPractice
            {
                NameProfModule = "ПМ.13 \"Профессиональный модуль\"",
                NamePractice = "ПП.13.01 \"Практика по профессиональному модулю\"",
                Specializaions = "09.02.07-П; 09.02.07-БД; 09.02.01"
            });

            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file practice.xlsx");
        }
    }
}

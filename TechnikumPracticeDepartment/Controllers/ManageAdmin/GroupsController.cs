using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using Org.BouncyCastle.Tls;
using OfficeOpenXml;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using CsvHelper;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Group = TechnikumPracticeDepartment.ModelsDB.Group;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class GroupsController : Controller
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
        
        TechnikumPracticeDepartmentContext db;
        public GroupsController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            var specializationList = db.Specializations.ToList();
            return View("~/Views/ManageAdmin/Groups/Index.cshtml", specializationList);
        }
        public async Task<IActionResult> GetList(int? typeSortListGroups, 
                                                 int? filterSpecialization, 
                                                 int? filterCourse, 
                                                 int? filterAvaliable, 
                                                 string search, int page = 1)
        {
            if (UpdateIn(1) == false)
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
                    Course = (nowDate > augustDate) ? (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation+1) 
                                                    : (a.YearOfGraduation <= yearNow ? a.YearOfGraduation - a.YearStartEducation : yearNow - a.YearStartEducation),
                    SpecializationName = a.Specialization.SpecializationCode + ". " + a.Specialization.SpecializationName,
                    Specialization_ID = a.SpecializationId,
                    YearStartEducation = a.YearStartEducation,
                    YearOfGraduation = a.YearOfGraduation
                }).ToList();

            int pageSize = 15;
            IQueryable<GroupsModel> Blocks = listGroups.AsQueryable();
            if(typeSortListGroups == 1)
                Blocks = Blocks.OrderBy(p => p.NameGroups);
            else if(typeSortListGroups == 2)
                Blocks = Blocks.OrderBy(p => p.Specialization_ID);
            else if(typeSortListGroups == 3)
                Blocks = Blocks.OrderBy(p => p.Course);
            if(filterSpecialization > 0)
                Blocks = Blocks.Where(p => p.Specialization_ID == filterSpecialization);
            if(filterCourse > 0)
                Blocks = Blocks.Where(p => p.Course == filterCourse);
            if(filterAvaliable > 0)
                Blocks = Blocks.Where(p => p.IsEnded == (filterAvaliable == 1 ? false : true));
            if (!String.IsNullOrEmpty(search))
                Blocks = Blocks.Where(p => p.NameGroups.ToLower().Contains(search.ToLower()));
            var items = Blocks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var count = Blocks.Count();

            IndexGroupModel model = new IndexGroupModel();
            model.PageViewModel = new PageViewModel(count, page, pageSize);
            model.FilterViewModel = new FilterViewModel_Group(typeSortListGroups, filterSpecialization, filterCourse, filterAvaliable, search);
            model.list_groups = items;

            return PartialView("~/Views/ManageAdmin/Groups/_GroupsList.cshtml", model);
        }
        public IActionResult downloadImportExample()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            System.IO.MemoryStream content = new System.IO.MemoryStream();
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Группы");
            List<ExportModelGroups> listExmple = new List<ExportModelGroups>();
            listExmple.Add(new ExportModelGroups
            {
                NameGroup = "П50-1-19",
                YearStartEducatuin = "2019",
                YearToEndEducation = "4",
                Code_Specialization = "09.02.07-П"
            });
            listExmple.Add(new ExportModelGroups
            {
                NameGroup = "ВД50-1-21",
                YearStartEducatuin = "2021",
                YearToEndEducation = "4",
                Code_Specialization = "09.02.07-ВД"
            });
            listExmple.Add(new ExportModelGroups
            {
                NameGroup = "Ю-1-20",
                YearStartEducatuin = "2020",
                YearToEndEducation = "4",
                Code_Specialization = "40.02.01"
            });
            ws.Cells["A1"].LoadFromCollection(listExmple, true);
            var exportbytes = pack.GetAsByteArray();
            return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Example import file groups.xlsx");
        }
        public IActionResult downloadExportGroups(int format, int? typeSortListGroups, int? filterSpecialization, int? filterCourse, int? filterAvaliable)
        {
            if (UpdateIn(1) == false)
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
                SpecializationName = a.Specialization.SpecializationCode,
                Specialization_ID = a.SpecializationId,
                YearStartEducation = a.YearStartEducation,
                YearOfGraduation = a.YearOfGraduation
            }).ToList();
            IQueryable<GroupsModel> Blocks = listGroups.AsQueryable();
            if (typeSortListGroups == 1)
                Blocks = Blocks.OrderBy(p => p.NameGroups);
            else if (typeSortListGroups == 2)
                Blocks = Blocks.OrderBy(p => p.Specialization_ID);
            else if (typeSortListGroups == 3)
                Blocks = Blocks.OrderBy(p => p.Course);
            if (filterSpecialization > 0)
                Blocks = Blocks.Where(p => p.Specialization_ID == filterSpecialization);
            if (filterCourse > 0)
                Blocks = Blocks.Where(p => p.Course == filterCourse);
            if (filterAvaliable > 0)
                Blocks = Blocks.Where(p => p.IsEnded == (filterAvaliable == 1 ? false : true));
            listGroups = Blocks.ToList();
            var Groups = listGroups.Select(p => new ExportModelGroups()
            {
                NameGroup = p.NameGroups,
                Code_Specialization = p.SpecializationName,
                YearStartEducatuin = p.YearStartEducation.ToString(),
                YearToEndEducation = (p.YearOfGraduation - p.YearStartEducation).ToString()
            });
            if (format == 1)
            {
                System.IO.MemoryStream content = new System.IO.MemoryStream();
                using (var writer = new StreamWriter(content, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, false))
                {
                    csv.WriteRecords(Groups);
                }
                return File(content.ToArray(), "text/csv", "Export groups " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".csv");
            }
            else
            {
                using ExcelPackage pack = new ExcelPackage();
                ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Группы");
                ws.Cells["A1"].LoadFromCollection(Groups, true);
                var exportbytes = pack.GetAsByteArray();
                return File(exportbytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Export groups " + DateTime.Now.ToLocalTime().ToString().Replace(":", ".") + ".xlsx");
            }
        }
        public bool CheckGroupName(int ID_Group, string NameGroup)
        {
            if (db.Groups.Where(p => p.NameGroup.ToLower() == NameGroup.ToLower() && p.IdGroup != ID_Group).Count() > 0)
            {
                return false;
            }
            return true;
        }
        public bool CheckYearStartEducation(string YearStartEducation)
        {
            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            try
            {
                int yearStart = Convert.ToInt16(YearStartEducation);
                if (yearStart == yearNow)
                    return true;
                else if (yearStart == yearNow - 3)
                    return true;
                else if (yearStart == yearNow - 2)
                    return true;
                else if (yearStart == yearNow - 1)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }
        public async Task<IActionResult> AddEditGroup(int id)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            AddEditGroup model = new AddEditGroup();
            if (id != 0)
            {
                var group = db.Groups.Include(p => p.Specialization).Where(p => p.IdGroup == id).First();
                DateTime nowDate = DateTime.Now;
                short yearNow = (short)nowDate.Year;
                DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
                model.Course = (nowDate > augustDate) ? (group.YearOfGraduation <= yearNow ? group.YearOfGraduation - group.YearStartEducation : yearNow - group.YearStartEducation + 1).ToString()
                                : (group.YearOfGraduation <= yearNow ? group.YearOfGraduation - group.YearStartEducation : yearNow - group.YearStartEducation).ToString();
                model.ID_Group = group.IdGroup;
                model.NameGroup = group.NameGroup;
                model.YearStartEducation = group.YearStartEducation.ToString();
                model.YearOfEducation = (group.YearOfGraduation - group.YearStartEducation).ToString();
                model.SelectedSpecialization = group.SpecializationId;
                model.SelectedSpecialization_Name = group.Specialization.SpecializationCode + ". " + group.Specialization.SpecializationName;
                model.list_specialization = db.Specializations.ToList();
                var studentsInGroup = db.Students.Include(p => p.Group).Include(p => p.User).Where(p => p.GroupId == id).Select(p => new StudentsOfGroup
                {
                    Selected = true,
                    SurnameStudent = p.User.SurnameUser,
                    NameStudent = p.User.NameUser,
                    PatronymicNameStudent = p.User.PatronymicNameUser,
                    ID_Student = p.IdStudent,
                    Group_name = p.Group.NameGroup,
                    group_ID = p.GroupId,
                    IsStudent = p.IsStudent
                }).ToList();
                var students = studentsInGroup.ToList();
                students = students.Where(p => p.Selected == true).ToList();
                model.students = students.OrderByDescending(p=>p.Selected).ThenBy(p=>p.group_ID).ThenBy(p=>p.SurnameStudent).ToList();
            }
            else
            {
                model.ID_Group = 0;
                model.list_specialization = db.Specializations.ToList();
            }
            return View("~/Views/ManageAdmin/Groups/AddEditGroup.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEditGroupPost(int id, AddEditGroup model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (id != 0)
            {
                if (ModelState.IsValid)
                {
                    Group group = new Group();
                    group = db.Groups.Find(id);
                    group.NameGroup = model.NameGroup;
                    group.SpecializationId = model.SelectedSpecialization;
                    group.YearStartEducation = Convert.ToInt16(model.YearStartEducation);
                    group.YearOfGraduation = Convert.ToInt16(Convert.ToInt16(model.YearStartEducation) + Convert.ToInt16(model.YearOfEducation));
                    db.Groups.Update(group);
                    db.SaveChanges();
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Group group = new Group();
                    group.NameGroup = model.NameGroup;
                    group.SpecializationId = model.SelectedSpecialization;
                    group.YearStartEducation = Convert.ToInt16(model.YearStartEducation);
                    group.YearOfGraduation = Convert.ToInt16(Convert.ToInt16(model.YearStartEducation) + Convert.ToInt16(model.YearOfEducation));
                    db.Groups.Add(group);
                    db.SaveChanges();
                    id = group.IdGroup;
                }
            }
            return RedirectToAction("AddEditGroup", "Groups", new { id = id });
        }
        [HttpPost]
        public async Task<IActionResult> EditGroupStudents(int id, AddEditGroup model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if (model.students == null)
                return RedirectToAction("AddEditGroup", "Groups", new { id = id });

            var students = model.students;
            var students_db = db.Students.Include(p=>p.User).Where(p => p.User.IsAvaliable == null || p.User.IsAvaliable == true == true).ToList();

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
                SpecializationName = a.Specialization.SpecializationCode,
                Specialization_ID = a.SpecializationId,
                YearStartEducation = a.YearStartEducation,
                YearOfGraduation = a.YearOfGraduation
            }).ToList();
            var frist_group = listGroups.Where(p => p.ID_Group != id && p.IsEnded == false).OrderBy(p => p.NameGroups).ToList();

            int frist_grp = frist_group.First().ID_Group;
            if (frist_group.Count <= 0)
            {
                var frist_groupss = db.Groups.ToList();
                frist_grp = frist_groupss.First().IdGroup;
            }

            foreach (var a in students)
            {
                if (a.Selected == true)
                {
                    if (a.group_ID == students_db.Where(p => p.IdStudent == a.ID_Student).First().GroupId && a.group_ID == id)
                    { }
                    else
                    {
                        Student student = students_db.Where(p => p.IdStudent == a.ID_Student).First();
                        student.GroupId = id;
                        db.Students.Update(student);
                    }
                }
                else
                {
                    if (a.group_ID == students_db.Where(p => p.IdStudent == a.ID_Student).First().GroupId && a.group_ID != id)
                    { }
                    else
                    {
                        Student student = students_db.Where(p => p.IdStudent == a.ID_Student).First();
                        student.GroupId = frist_grp;
                        db.Students.Update(student);
                    }
                }
            }
            db.SaveChanges();

            return RedirectToAction("AddEditGroup", "Groups", new { id = id });
        }
        public IActionResult ImportGroups()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            return View("~/Views/ManageAdmin/Groups/ImportGroups.cshtml", new ImportFileGroups());
        }
        public CheckImport checkModel(AddEditGroup model)
        {
            string regex_groupName = "^([А-Я]{1})([А-Я]{1})?([1-9]{1}[0-9]{1})?-[1-9]{1}([0-9])?-[1-2]{1}[0-9]{1}((,{1})( {1})?([А-Я]{1})([А-Я]{1})?([1-9]{1}[0-9]{1})?-11/[1-9]{1}([0-9])?-[1-2]{1}[0-9]{1})?$";
            string regex_YearStart = "[2][0]{1}[1-2]{1}[0-9]{1}";
            CheckImport check = new CheckImport();
            check.checker = true;

            if (!Regex.IsMatch(model.NameGroup, regex_groupName))
            {
                check.checker = false;
                check.Errors += "Наименование группы указано некорректно ";
            }
            if (!Regex.IsMatch(model.YearStartEducation, regex_YearStart))
            {
                check.checker = false;
                check.Errors += "Год начала обучения указан некорректно ";
            }
            if (!CheckYearStartEducation(model.YearStartEducation))
            {
                check.checker = false;
                check.Errors += "Год начала обучения указан некорректно ";
            }
            try
            {
                int years = Convert.ToInt16(model.YearOfEducation);
                if(years < 2 || years > 6)
                {
                    check.checker = false;
                    check.Errors += "Количество лет обучения должно быть от 2 до 6 лет ";
                }
            }
            catch
            {
                check.checker = false;
                check.Errors += "Количество лет обучения указано некорректно ";
            }
            
            if(!CheckGroupName(0,model.NameGroup))
            {
                check.checker = false;
                check.Errors += "Указанная группа уже существует ";
            }

            if (db.Specializations.Where(p => p.SpecializationCode.Trim() == model.SelectedSpecialization_Name.Trim().ToLower()).Count() <= 0)
            {
                check.checker = false;
                check.Errors += "Указанная специальность не найдена ";
            }

            return check;
        }

        [HttpPost]
        public async Task<IActionResult> ImportGroups(ImportFileGroups model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            model.notPassedGroups = new List<AddEditGroup>();
            model.passedGroups = new List<AddEditGroup>();
            model.errorsImport = new List<CheckImport>();

            if (model.UploadedFile?.Length > 0)
            {
                var stream = model.UploadedFile.OpenReadStream();
                List<AddEditGroup> notPassedGroups = new List<AddEditGroup>();
                List<AddEditGroup> passedGroups = new List<AddEditGroup>();
                List<CheckImport> errorsImport = new List<CheckImport>();
                var specializations = db.Specializations.ToList();
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
                                var nameGroup = worksheet.Cells[row, 1].Value?.ToString();
                                var codeSpecialization = worksheet.Cells[row, 2].Value?.ToString();
                                var yearStartEducation = worksheet.Cells[row, 3].Value?.ToString();
                                var yearsOfEducation = worksheet.Cells[row, 4].Value?.ToString();

                                var element = new AddEditGroup()
                                {
                                    NameGroup = nameGroup == null ? " " : nameGroup,
                                    SelectedSpecialization_Name = codeSpecialization == null ? " " : codeSpecialization,
                                    YearStartEducation = yearStartEducation == null ? " " : yearStartEducation,
                                    YearOfEducation = yearsOfEducation == null ? " " : yearsOfEducation
                                };
                                CheckImport check = checkModel(element);

                                if (check.checker == false)
                                {
                                    notPassedGroups.Add(element);
                                    errorsImport.Add(check);
                                }
                                else
                                {
                                    passedGroups.Add(element);

                                    ModelsDB.Group group = new ModelsDB.Group();
                                    group.NameGroup = element.NameGroup;
                                    group.YearStartEducation = Convert.ToInt16(element.YearStartEducation);
                                    group.YearOfGraduation = Convert.ToInt16(Convert.ToInt16(element.YearStartEducation) + Convert.ToInt16(element.YearOfEducation));
                                    group.SpecializationId = specializations.Where(p => p.SpecializationCode.Trim() == element.SelectedSpecialization_Name.Trim().ToLower()).First().IdSpecialization;
                                    db.Groups.Add(group);
                                    db.SaveChanges();
                                }
                            }
                            catch
                            {
                                ImportFileGroups models = new ImportFileGroups();
                                models.notPassedGroups = new List<AddEditGroup>();
                                models.passedGroups = new List<AddEditGroup>();
                                models.errorsImport = new List<CheckImport>();
                                models.UploadedFile = model.UploadedFile;
                                models.IsError = true;
                                models.ErrorTitle = "Ошибка!";
                                models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                                models.notPassedGroups = notPassedGroups;
                                models.passedGroups = passedGroups;
                                models.errorsImport = errorsImport;
                                db.SaveChanges();
                                return View("~/Views/ManageAdmin/Groups/ImportGroups.cshtml", models);
                            }
                        }
                    }
                    model.notPassedGroups = notPassedGroups;
                    model.passedGroups = passedGroups;
                    model.errorsImport = errorsImport;
                }
                catch (Exception e)
                {
                    ImportFileGroups models = new ImportFileGroups();
                    models.notPassedGroups = new List<AddEditGroup>();
                    models.passedGroups = new List<AddEditGroup>();
                    models.errorsImport = new List<CheckImport>();
                    models.UploadedFile = model.UploadedFile;
                    models.IsError = true;
                    models.ErrorTitle = "Ошибка!";
                    models.ErrorMessage = "Произошла непредвиденная ошибка! Попробуйте выбрать другой файл или повторить попытку позже";
                    models.notPassedGroups = notPassedGroups;
                    models.passedGroups = passedGroups;
                    models.errorsImport = errorsImport;
                    return View("~/Views/ManageAdmin/Groups/ImportGroups.cshtml", models);
                }
            }
            db.SaveChanges();
            return View("~/Views/ManageAdmin/Groups/ImportGroups.cshtml", model);
        }
    }
}

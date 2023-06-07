using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Security.Claims;
using TechnikumPracticeDepartment.Models.ModelsResumeStudent;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Controllers.StudentPage
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class StudentResumeController : Controller
    {
        TechnikumPracticeDepartmentContext db;
        private IConfiguration configuration;
        public StudentResumeController(TechnikumPracticeDepartmentContext context, IConfiguration conf)
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
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            ResumeModel resumeModel = new ResumeModel();
            var userDB = db.Users.Include(p=>p.Student).Include(p=>p.Student.Resume).Include(p=>p.Student.Group).Include(p=>p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

            DateTime nowDate = DateTime.Now;
            short yearNow = (short)nowDate.Year;
            DateTime augustDate = new DateTime(nowDate.Year, 8, 1);
            int Course = (nowDate > augustDate) ? (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation + 1)
                                : (userDB.Student.Group.YearOfGraduation <= yearNow ? userDB.Student.Group.YearOfGraduation - userDB.Student.Group.YearStartEducation : yearNow - userDB.Student.Group.YearStartEducation);
            resumeModel.course = Course.ToString();
            resumeModel.student_info = userDB.Student;

            if(userDB.Student.Resume != null)
            {
                resumeModel.WorkExperience = userDB.Student.Resume.WorkExperience;
                resumeModel.EducationInfo = userDB.Student.Resume.Education;
                resumeModel.AdditionalInfo = userDB.Student.Resume.AdditionalInformation;
                resumeModel.About = userDB.Student.Resume.AboutStudent;
                resumeModel.Dolzhnost = userDB.Student.Resume.DesiredPosition;
                resumeModel.ProffessionalSkills = userDB.Student.Resume.ProfessionalSkills;
                resumeModel.tags = userDB.Student.Resume.TagsSkills.Split(";");
            }

            return View("~/Views/StudentPage/StudentResume/Index.cshtml", resumeModel);
        }
        public bool checkEducationInfo(string EducationInfo)
        {
            int howMuchN = EducationInfo.Count(x => x == '\n');
            if(howMuchN > 1)
            {
                for (int i = 0; i < EducationInfo.Length; i++)
                {
                    try
                    {
                        if (EducationInfo[i] == '\n' && EducationInfo[i + 1] == '\n' && EducationInfo[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (EducationInfo[EducationInfo.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if(EducationInfo.Trim().Length < 10 || EducationInfo.Trim().Length > 1000)
                return false;
            return true;
        }
        public bool checkAbout(string About)
        {
            int howMuchN = About.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < About.Length; i++)
                {
                    try
                    {
                        if (About[i] == '\n' && About[i + 1] == '\n' && About[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (About[About.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (About.Trim().Length < 10 || About.Trim().Length > 1000)
                return false;
            return true;
        }
        public bool checkDolzhnost(string Dolzhnost)
        {
            if (Dolzhnost.Trim().Length < 4 || Dolzhnost.Trim().Length > 50)
                return false;
            if (Dolzhnost.Contains("\n"))
                return false;
            return true;
        }
        public bool checkWorkExperience(string WorkExperience)
        {
            int howMuchN = WorkExperience.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < WorkExperience.Length; i++)
                {
                    try
                    {
                        if (WorkExperience[i] == '\n' && WorkExperience[i + 1] == '\n' && WorkExperience[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (WorkExperience[WorkExperience.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (WorkExperience.Trim().Length < 10 || WorkExperience.Trim().Length > 1000)
                return false;
            return true;
        }
        public bool checkProffessionalSkills(string ProffessionalSkills)
        {
            int howMuchN = ProffessionalSkills.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < ProffessionalSkills.Length; i++)
                {
                    try
                    {
                        if (ProffessionalSkills[i] == '\n' && ProffessionalSkills[i + 1] == '\n' && ProffessionalSkills[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (ProffessionalSkills[ProffessionalSkills.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (ProffessionalSkills.Trim().Length < 10 || ProffessionalSkills.Trim().Length > 1000)
                return false;
            return true;
        }
        public bool checkAdditionalInfo(string AdditionalInfo)
        {
            int howMuchN = AdditionalInfo.Count(x => x == '\n');
            if (howMuchN > 1)
            {
                for (int i = 0; i < AdditionalInfo.Length; i++)
                {
                    try
                    {
                        if (AdditionalInfo[i] == '\n' && AdditionalInfo[i + 1] == '\n' && AdditionalInfo[i + 2] == '\n')
                            return false;
                    }
                    catch
                    {
                        if (AdditionalInfo[AdditionalInfo.Length - 1] == '\n')
                            return false;
                    }
                }
            }
            if (AdditionalInfo.Trim().Length < 10 || AdditionalInfo.Trim().Length > 1000)
                return false;
            return true;
        }

        [HttpPost]
        public IActionResult ResumeUpdate(ResumeModel model, IFormFile fileUploaded)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
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
                return View("~/Views/StudentPage/StudentResume/Index.cshtml", resumeModel);
            }
            Resume resume = new Resume();
            if(userDB.Student.Resume != null)
                resume = userDB.Student.Resume;

            resume.Education = model.EducationInfo.Trim();
            resume.DesiredPosition = model.Dolzhnost.Trim();
            resume.AboutStudent = model.About.Trim();
            resume.WorkExperience = model.WorkExperience.Trim();
            resume.ProfessionalSkills = model.ProffessionalSkills.Trim();
            resume.AdditionalInformation = model.AdditionalInfo.Trim();
            resume.TagsSkills = string.Join(";",model.tags);
            resume.Student = userDB.Student;
            if(fileUploaded != null)
            {
                resume.FileWithResume = userDB.Student.IdStudent.ToString()+"/"+userDB.Student.IdStudent.ToString() + ".pdf";
                SendFileToServer sendFileTo = new SendFileToServer(configuration);
                _ = sendFileTo.SendFileImageStudent(fileUploaded, userDB.Student.IdStudent.ToString(), ".pdf");
            }

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
            return RedirectToAction("Index");
        }
        public IActionResult HideResumeOpen(int value)
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            if(userDB.Student.Resume != null)
            {
                userDB.Student.Resume.IsAvaliable = value == 0 ? false : true;
                db.Update(userDB.Student.Resume);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public IActionResult DeleteResume()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
            if (userDB.Student.Resume != null)
            {
                if (userDB.Student.Resume.FileWithResume != null)
                {
                    SendFileToServer sendFileTo = new SendFileToServer(configuration);
                    sendFileTo.DeleteOldFile(userDB.Student.Resume.FileWithResume, 1);
                    userDB.Student.Resume.FileWithResume = null;
                    db.Update(userDB.Student.Resume);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult LookResume()
        {
            if (UpdateIn(4) == false)
                return RedirectToAction("Index", "Home");
            ResumeModel resumeModel = new ResumeModel();
            var userDB = db.Users.Include(p => p.Student).Include(p => p.Student.Resume).Include(p => p.Student.Group).Include(p => p.Student.Group.Specialization).Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();

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
            else
                return RedirectToAction("Index");

            return View("~/Views/StudentPage/StudentResume/LookResume.cshtml", resumeModel);
        }
    }
}

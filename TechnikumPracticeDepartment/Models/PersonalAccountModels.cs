using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models
{
    public class updateImageStudent
    {
        [Required(ErrorMessage = "Загрузите изображение")]
        public string FTTPPathImage { get; set; }

        [ValidateNever]
        public IFormFile uploadedImage { get; set; }
    }
    public class PersonalAccountModels
    {
        //Для всех пользователей
        public string SurnameUser { get; set; }
        public string NameUser { get; set; }
        public string Email { get; set; }
        public string? PatronymicnameUser { get; set; }

        //Для администратора, сотрудника производственного отдела
        public string? roles { get; set; }

        //Для студеннта
        public string? GroupName { get; set; }
        public string? SpecializationCode { get; set; }
        public string? SpecializationName { get; set; }
        public string? dateOfBirthday { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PathImageStudent { get; set; }
        public List<PracticesStudentWithDistribution>? list_practices { get; set; }

        //Для сотрудников организаций
        public Organization? info_organization { get; set; }
        //public List<PracticeChartDistibution>? list_practiceDistribution { get; set; }
        public bool? IsUpdatesPassword { get; set; }
    }
    public class PracticesStudentWithDistribution
    {
        public int ID_Practice { get; set; }
        public string NameProfModule { get; set; }
        public string NamePractice { get; set; }
        public string Hours { get; set; }
        public bool IsEnded { get; set; }
        public List<PracticeChart> list_periods { get; set; }
        public Organization organization { get; set; }
    }
    public class UpdatePasswordModel
    {
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Длина пароля указана некорректно")]
        [Remote("CheckPassword", "Home", ErrorMessage = "Указанный пароль не соотвествует требованиям безопасности (минимум 8 символов, буквы, минимум 1 спец. символ, цифры)")]
        public string newPassword { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Длина пароля указана некорректно")]
        [Compare("newPassword", ErrorMessage = "Указанное подтверждение пароля не совпадает с указанным выше паролем")]
        public string compareNewPassword { get; set; }
    }
    public class UpdateStudentInfoModel
    {
        [ValidateNever]
        public int Id_Student { get; set; }

        [Remote("CheckDateOfBirthday", "Home", ErrorMessage = "Дата рождения указана некорректно")]
        public string? dateOfBirthday { get; set; }

        [Remote("CheckPhoneNumber", "Home", AdditionalFields = "Id_Student", ErrorMessage = "Номер телефона указан некорректно")]
        public string? phoneNumber { get; set; }
    }
}

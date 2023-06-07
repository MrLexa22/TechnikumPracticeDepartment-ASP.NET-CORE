using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TechnikumPracticeDepartment.Models.ModelsStudentsPages
{
    public class AddEditStudent
    {
        [ValidateNever]
        public int ID_Student { get; set; }

        [ValidateNever]
        public int ID_User { get; set; }

        [ValidateNever]
        public bool resum_is { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Длина email указана некорректно")]
        [Remote("CheckUserEmail", "Employees", AdditionalFields = "ID_User", ErrorMessage = "Пользователь с таким email уже существует")]
        public string Email { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameUser { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени указана некорректно")]
        public string NameUser { get; set; }

        [Remote("CheckPatronymicNameUser", "Employees", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameUser { get; set; }

        [ValidateNever]
        public List<ModelsDB.Group> list_groups { get; set; }

        [ValidateNever]
        public int SelectedGroup_ID { get; set; }

        [Remote("CheckPhoneNumber", "Students", AdditionalFields = "ID_Student", ErrorMessage = "Номер телефона указа некорректно / Студент с таким номером телефона уже существует")]
        public string? PhoneNumber { get; set; }

        [Remote("CheckDateOfBirthday", "Students", ErrorMessage = "Некорректная дата рождения")]
        public string? DateOfBirthday { get; set; }

        [ValidateNever]
        public bool? IsStudent { get; set; }

        [ValidateNever]
        public bool? IsEnded { get; set; }

        [ValidateNever]
        public string? PathImageStudent { get; set; }
    }
}

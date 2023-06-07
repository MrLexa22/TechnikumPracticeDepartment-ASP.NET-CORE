using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TechnikumPracticeDepartment.Models.ModelsEmployeesPages
{
    public class AddEditEmployee
    {
        [ValidateNever]
        public int ID_User { get; set; }

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

        [Remote("CheckSelectedRoles", "Employees", AdditionalFields = "Employee, Administrator", ErrorMessage = "Выберите хотя бы одну роль")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string rolecheck { get; set; }
        public bool Administrator { get; set; }
        public bool Employee { get; set; }
        public bool? IsAvaliable { get; set; }
    }
}

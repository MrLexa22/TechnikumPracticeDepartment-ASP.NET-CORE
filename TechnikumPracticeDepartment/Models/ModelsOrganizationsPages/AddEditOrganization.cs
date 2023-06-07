using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationsPages
{
    public class AddEditOrganization
    {
        [ValidateNever]
        public int ID_Organization { get; set; }

        [ValidateNever]
        public List<Vacancy> vacancy_list { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Длина полного наименования организации некорректна")]
        [RegularExpression("^[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Наименование поного наименования организации указано некорректно")]
        [Remote("CheckFullNameOrganization", "Organizations", AdditionalFields = "ID_Organization", ErrorMessage = "Такая организация уже существует")]
        public string FullNameOrganization { get; set; }

        [RegularExpression("^[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Наименование сокращённого наименования организации указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Длина сокращённого наименования организации некорректна")]
        [Remote("CheckNotFullNameOrganization", "Organizations", AdditionalFields = "ID_Organization", ErrorMessage = "Такая организация уже существует")]
        public string NotFullNameOrganization { get; set; }

        [RegularExpression("^[0-9]{6},[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Адрес организации указан некорректно (пример: 101000, г. Москва)")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Адрес организации указа некорректно")]
        public string AddressOrganization { get; set; }

        [Remote("CheckInnOrganization", "Organizations", AdditionalFields = "ID_Organization", ErrorMessage = "ИНН указа некорректно / уже существует")]
        public string? INNOrganization { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameUser { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина имени указана некорректно")]
        public string NameUser { get; set; }

        [Remote("CheckPatronymicNameUser", "Employees", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameUser { get; set; }

        [Remote("CheckPhoneNumber", "Organizations", AdditionalFields = "ID_Organization", ErrorMessage = "Номер телефона указа некорректно / Организация с таким номером телефона уже существует")]
        public string? PhoneNumber { get; set; }

        [ValidateNever]
        public List<EmployeeOfOrganization> employeesOrganization { get; set; }

        [ValidateNever]
        public bool? IsAvaliable { get; set; }
    }
    public class AddEditOrganizationAccount
    {
        [ValidateNever]
        public int ID_Organization { get; set; }

        [ValidateNever]
        public int ID_User { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameUser { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина имени указана некорректно")]
        public string NameUser { get; set; }

        [Remote("CheckPatronymicNameUser", "Employees", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameUser { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Длина email указана некорректно")]
        [Remote("CheckUserEmail", "Employees", AdditionalFields = "ID_User", ErrorMessage = "Пользователь с таким email уже существует")]
        public string Email { get; set; }
        
        [ValidateNever]
        public string SurnameContact { get; set; }

        [ValidateNever]
        public string NameContact { get; set; }

        [ValidateNever]
        public string? PatronymicnameContact { get; set; }

        [ValidateNever]
        public bool IsContact { get; set; }

        [ValidateNever]
        public bool SendEmail { get; set; }
    }
}

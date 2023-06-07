using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsOrganizationPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsStudents
{
    public class IndexStudentsResponses
    {
        public List<ResponseFromStudent> responses_fromStudent { get; set; }
        public List<ResponseFromOrganization> responses_fromOrganization { get; set; }
    }
    public class ResponseDistribution
    {
        [ValidateNever]
        public int IdRequest { get; set; }

        [ValidateNever]
        public RequestToDistributuion request { get; set; }

        [ValidateNever]
        public bool isExist { get; set; }

        [ValidateNever]
        public Organization? existOrganization { get; set; }

        [ValidateNever]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckInnOrganization", "Organizations", AdditionalFields = "ID_Organization", ErrorMessage = "ИНН указа некорректно / уже существует (обратитесь в отдел производственного обучения)")]
        public string InnOrganization { get; set; }

        [RegularExpression("^[0-9]{6},[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Адрес организации указан некорректно (пример: 101000, г. Москва)")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Адрес организации указа некорректно")]
        public string AddressOrganization { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Длина полного наименования организации некорректна")]
        [RegularExpression("^[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Наименование поного наименования организации указано некорректно")]
        [Remote("CheckFullNameOrganization", "Organizations", ErrorMessage = "Такая организация уже существует (обратитесь в отдел производственного обучения)")]
        public string FullNameOrganization { get; set; }

        [RegularExpression("^[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Наименование сокращённого наименования организации указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Длина сокращённого наименования организации некорректна")]
        [Remote("CheckNotFullNameOrganization", "Organizations", ErrorMessage = "Такая организация уже существует (обратитесь в отдел производственного обучения)")]
        public string NotFullNameOrganization { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameContactNameOrganization { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени указана некорректно")]
        public string NameContactNameOrganization { get; set; }

        [Remote("CheckPatronymicNameUser", "Employees", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameContactNameOrganization { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Длина email указана некорректно")]
        [Remote("CheckUserEmail", "Employees", ErrorMessage = "Пользователь с таким email уже существует (обратитесь в отдел производственного обучения)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckPhoneNumber", "Organizations", ErrorMessage = "Номер телефона указа некорректно / Организация с таким номером телефона уже существует (обратитесь в отдел производственного обучения)")]
        public string PhoneNumber { get; set; }

        [ValidateNever]
        public int StatusReuqest { get; set; }

        [ValidateNever]
        public User EmployeeOfTechnikum { get; set; }
    }
}

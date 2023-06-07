using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Models.ModelsDistributionStudentsPages;

namespace TechnikumPracticeDepartment.Models.ModelsManageResponses
{
    public class IndexManageResponses
    {
        public List<ResponseFromOrganizationOrStudent> list_Responses { get; set; }
        public List<RequestToDistributuion> list_Requests { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_ManageResponses FilterViewModel { get; set; }
    }
    public class ResponseFromOrganizationOrStudent
    {
        [ValidateNever]
        public ResponseFromOrganization? ResponseFromOrganization { get; set; }

        [ValidateNever]
        public ResponseFromStudent? ResponseFromStudent { get; set; }

        [ValidateNever]
        public string? CommentAccept { get; set; }

        [ValidateNever]
        public int StatusResponse { get; set; }

        [ValidateNever]
        public string? CommentDiscard { get; set; }

        [ValidateNever]
        public string? ErrorText { get; set; }

        [ValidateNever]
        public int typeResponse { get; set; }

        [ValidateNever]
        public int IdResponse { get; set; }

        [ValidateNever]
        public List<Practice_DistributionStudent_Boolean> list_practice { get; set; }

        [ValidateNever]
        public bool error { get; set; }
    }
    public class Practice_DistributionStudent_Boolean : Practice_DistributionStudent
    {
        public bool Selected { get; set; }
    }
    public class FilterViewModel_ManageResponses
    {
        public FilterViewModel_ManageResponses(int? sortList, int? filterListType, int? filterListStatus, string search)
        {
            SortList = sortList;
            FilterListType = filterListType;
            FilterListStatus = filterListStatus;
            Search = search;
        }
        public int? SortList { get; private set; }
        public int? FilterListType { get; private set; }
        public int? FilterListStatus { get; private set; }
        public string Search { get; private set; }
    }
    public class ResponseDistributionFromStudent
    {
        [ValidateNever]
        public int IdRequest { get; set; }

        [ValidateNever]
        public List<Practice_DistributionStudent> list_practice { get; set; }

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
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameContactNameOrganization { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина имени указана некорректно")]
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
        public User? EmployeeOfTechnikum { get; set; }
    }
}

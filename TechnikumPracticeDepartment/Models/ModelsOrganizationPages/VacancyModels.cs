using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.Models.ModelsStudentsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationPages
{
    public class VacancyModels
    {
        public List<Vacancy> list_vacancy { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Vacancy FilterViewModel { get; set; }
    }

    public class FilterViewModel_Vacancy
    {
        public FilterViewModel_Vacancy(int? typeSortList, string? filterTags, string search)
        {
            TypeSortList = typeSortList;
            FilterTags = filterTags;
            Search = search;
        }
        public int? TypeSortList { get; private set; }
        public string? FilterTags { get; private set; }
        public string Search { get; private set; }
    }
    public class AddEditVacancy
    {
        [ValidateNever]
        public Organization organization { get; set; }

        [ValidateNever]
        public int organizationID { get; set; }

        [ValidateNever]
        public int responseStudent { get; set; }

        [ValidateNever]
        public int responseId { get; set; }

        [ValidateNever]
        public int IdVacancy { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [MinLength(4, ErrorMessage = "Минимум 4 символов")]
        [MaxLength(50, ErrorMessage = "Максимум 50 символов")]
        [Remote("checkNameVacancy", "ManageVacancy", AdditionalFields = "IdVacancy, organizationID", ErrorMessage = "Некорректная длина текста (данное название вакансии уже существует)")]
        public string NameVacancy { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Максимум 2000 символов")]
        [Remote("checkDescription", "ManageVacancy", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Максимум 2000 символов")]
        [Remote("checkDuties", "ManageVacancy", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string Duties { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Максимум 2000 символов")]
        [Remote("checkRequirements", "ManageVacancy", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string Requirements { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Максимум 2000 символов")]
        [Remote("checkConditions", "ManageVacancy", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string Conditions { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Максимум 2000 символов")]
        [Remote("checkAdditionalInformation", "ManageVacancy", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string AdditionalInformation { get; set; }

        [ValidateNever]
        public List<String> Tags { get; set; }

        [Required(ErrorMessage = "Укажите хотя бы один ключевой навык")]
        public string[] _tags { get; set; }

        [ValidateNever]
        public List<StudentsReposnses> Responses_students { get; set; }

        [ValidateNever]
        public List<StudentsReposnses> Responses_organization { get; set; }
    }
    public class StudentsReposnses : Students
    {
        public string statusResponse { get; set; }
        public int status { get; set; }
        public ResponseFromStudent responseFromStudent { get; set; }
        public ResponseFromOrganization responseFromOrganization { get; set; }
    }
}

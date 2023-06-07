using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsResumeStudent
{
    public class ResumeModel
    {
        [ValidateNever]
        public Student student_info { get; set; }

        [ValidateNever]
        public string course { get; set; }

        [ValidateNever]
        public List<Vacancy> vacancies { get; set; }
        
        [ValidateNever]
        public int SelectedVacancy { get; set; }

        [ValidateNever]
        public string? Comment { get; set; }

        [ValidateNever]
        public List<ResponseFromOrganization>? responseFromOrganization { get; set; }

        [ValidateNever]
        public List<ResponseFromStudent>? responseFromStudent { get; set; }

        [Required(ErrorMessage = "Укажите хотя бы один ключевой навык")]
        public string[] tags { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10,ErrorMessage = "Минимум 10 символов")]
        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        [Remote("checkEducationInfo", "StudentResume", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string EducationInfo { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [MinLength(4, ErrorMessage = "Минимум 4 символов")]
        [MaxLength(50, ErrorMessage = "Максимум 50 символов")]
        [Remote("checkDolzhnost", "StudentResume", ErrorMessage = "Некорректная длина символов")]
        public string Dolzhnost { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        [Remote("checkAbout", "StudentResume", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string About { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        [Remote("checkWorkExperience", "StudentResume", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string WorkExperience { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        [Remote("checkProffessionalSkills", "StudentResume", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string ProffessionalSkills { get;set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения. Если информации нет, то укажите слово 'Отсутствует'")]
        [MinLength(10, ErrorMessage = "Минимум 10 символов")]
        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        [Remote("checkAdditionalInfo", "StudentResume", ErrorMessage = "Не допускается более одного Enter между строками. В конце Enter также не допустим! (некорректная длина)")]
        public string AdditionalInfo { get; set; }
    }
}

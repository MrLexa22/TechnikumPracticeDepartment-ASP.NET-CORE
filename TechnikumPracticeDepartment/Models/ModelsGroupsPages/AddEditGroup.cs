using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsGroupsPages
{
    public class AddEditGroup
    {
        [ValidateNever]
        public int ID_Group { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(25, MinimumLength = 4, ErrorMessage = "Длина наименования группы некорректна")]
        [RegularExpression("^([А-Я]{1})([А-Я]{1})?([1-9]{1}[0-9]{1})?-[1-9]{1}([0-9])?-[1-2]{1}[0-9]{1}((,{1})( {1})?([А-Я]{1})([А-Я]{1})?([1-9]{1}[0-9]{1})?-11/[1-9]{1}([0-9])?-[1-2]{1}[0-9]{1})?$", ErrorMessage = "Наименование группы указано некорректно")]
        [Remote("CheckGroupName", "Groups", AdditionalFields = "ID_Group", ErrorMessage = "Группа с таким названием уже существует")]
        public string NameGroup { get; set; }

        [RegularExpression("[2][0]{1}[1-2]{1}[0-9]{1}", ErrorMessage = "Год начала обучения указан некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Год начала обучения указан некорректно")]
        [Remote("CheckYearStartEducation", "Groups", ErrorMessage = "Год начала обучения указан некорректно")]
        public string YearStartEducation { get; set; }

        [RegularExpression("[2-6]{1}", ErrorMessage = "Количество лет обучения указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "Количество лет обучения указано некорректно")]
        public string YearOfEducation { get; set; }
        [ValidateNever]
        public List<Specialization> list_specialization { get; set; }
        [ValidateNever]
        public int SelectedSpecialization { get; set; }
        [ValidateNever]
        public string SelectedSpecialization_Name { get; set; }
        [ValidateNever]
        public string Course { get; set; }
        [ValidateNever]
        public List<StudentsOfGroup> students { get; set; }
    }
    public class StudentsOfGroup
    {
        public string SurnameStudent { get; set; }
        public string NameStudent { get; set; }
        public string? PatronymicNameStudent { get; set; }
        public int ID_Student { get; set; }
        public string Group_name { get; set; }
        public int group_ID { get; set; }
        public bool Selected { get; set; }
        public bool? IsStudent { get; set; }
    }
}

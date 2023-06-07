using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TechnikumPracticeDepartment.Models.ModelsSpecializationPages
{
    public class AddEditSpecializationModel
    {
        [ValidateNever]
        public int ID_Specialization { get; set; }

        [RegularExpression("[0-9]{2}.[0-9]{2}.[0-9]{2}(-[А-Я][А-Я]?)?", ErrorMessage = "Код специальности указан некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckSpecializationCode", "Specialization", AdditionalFields = "ID_Specialization", ErrorMessage = "Специальность с таким кодом уже существует")]
        public string SpecializationCode { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Наименование специальности указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 5,ErrorMessage = "Наименование специальности указано некорректно")]
        public string SpecializationName { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Квалификация специальности указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Квалификация специальности указано некорректно")]
        public string SpecizalizationQualif { get; set; }

        [ValidateNever]
        public List<GroupsSpecialization> list_groups {get;set;}
    }

    public class GroupsSpecialization
    {
        public string NameGroup { get; set; }
        public int ID_Group { get; set; }
        public string SpecName { get; set; }
        public int? Course { get; set; }
        public int SpecID { get; set; }
        public bool Selected { get; set; }
    }
}

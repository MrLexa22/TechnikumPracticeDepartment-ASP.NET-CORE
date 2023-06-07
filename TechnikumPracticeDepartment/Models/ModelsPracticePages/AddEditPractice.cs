using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.ModelsDB;
using TechnikumPracticeDepartment.Controllers.ManagePractice;

namespace TechnikumPracticeDepartment.Models.ModelsPracticePages
{
    public class AddEditPractice
    {
        [ValidateNever]
        public int ID_Practice { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Длина наименованию профессионального модуля указана некорректно")]
        [RegularExpression("^(ПМ.[0-9]{2}|ПДП.) (\"|«)[А-Яа-яЁё,. ()]{5,}(\"|»)$", ErrorMessage = "Некорректное наименовани проф. модуля. Пример: ПМ.11 \"(«)Название, название. Название\"(») ИЛИ ПДП \"(«)Название, название. Название\"(»)")]
        public string NameProfModuel { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Длина наименованию практики указана некорректно")]
        [RegularExpression("^(ПП.[0-9]{2}.[0-9]{2}|ПДП.) (\"|«)[А-Яа-яЁё,. ()]{5,}(\"|»)$", ErrorMessage = "Некорректное наименовани практики Пример: ПП.11.01 \"(«)Название, название. Название\"(») ИЛИ ПДП. \"(«)Название, название. Название\"(»)")]
        [Remote("CheckNamePractice", "Practice", AdditionalFields = "ID_Practice, NameProfModuel", ErrorMessage = "Такая практика с таким названием уже существует (ПП.XX, ПМ.XX: XX не равны)")]
        public string NamePractice { get; set; }

        [ValidateNever]
        public List<String> old_profModules { get; set; }

        [ValidateNever]
        public List<SpecializaionWithBool> list_specializaion { get; set; }

        [ValidateNever]
        public bool? IsAvaliableForDelete { get; set; }

        [ValidateNever]
        public bool? IsSaved { get; set; }
    }

    public class SpecializaionWithBool
    {
        public int ID_Specializaion { get; set; }
        public string NameSpecializaion { get; set; }
        public string CodeSpecializaion { get; set; }
        public bool IsSelected { get; set; }
    }
}

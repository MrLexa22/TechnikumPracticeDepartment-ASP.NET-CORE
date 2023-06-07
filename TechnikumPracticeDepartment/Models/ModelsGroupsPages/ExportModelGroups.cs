using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Models.ModelsGroupsPages
{
    public class ExportModelGroups
    {
        [Name("Наименование группы")]
        [Description("Наименование группы")]
        public string? NameGroup { get; set; }

        [Name("Код специальности")]
        [Description("Код специальности")]
        public string? Code_Specialization { get; set; }

        [Name("Год начала обучения")]
        [Description("Год начала обучения")]
        public string? YearStartEducatuin { get; set; }

        [Name("Количество лет обучения")]
        [Description("Количество лет обучения")]
        public string? YearToEndEducation { get; set; }
    }
    public class ImportFileGroups
    {
        [Required(ErrorMessage = "Выберите файл!")]
        public IFormFile UploadedFile { set; get; }

        [ValidateNever]
        public bool IsError { get; set; }

        [ValidateNever]
        public string ErrorTitle { get; set; }

        [ValidateNever]
        public string ErrorMessage { get; set; }

        [ValidateNever]
        public List<AddEditGroup> notPassedGroups { get; set; }

        [ValidateNever]
        public List<AddEditGroup> passedGroups { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }
}

using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsOrganizationsPages;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Models.ModelsPracticePages
{
    public class ExportModelPractice
    {
        [Name("Наименование профессионального модуля")]
        [Description("Наименование профессионального модуля")]
        public string NameProfModule { get; set; }

        [Name("Наименование практики")]
        [Description("Наименование практики")]
        public string NamePractice { get; set; }

        [Name("Код специальности(ей)")]
        [Description("Код специальности(ей)")]
        public string Specializaions { get; set; }
    }
    public class ImportFilePractice
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
        public List<AddEditPractice> notPassedPractices { get; set; }

        [ValidateNever]
        public List<AddEditPractice> passedPractices { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }
}

using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsSpecializationPages
{
    public class ExportModelSpecialization
    {
        [Name("Код специальности")]
        [Description("Код специальности")]
        public string CodeSpecialization { get; set; }

        [Name("Наименование специальности")]
        [Description("Наименование специальности")]
        public string NameSpecialization { get; set; }

        [Name("Наименование квалификации специальности")]
        [Description("Наименование квалификации специальности")]
        public string NameQualificationSpecialization { get; set; }
    }

    public class ImportFileSpecialization
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
        public List<AddEditSpecializationModel> notPassedSpecializations { get; set; }

        [ValidateNever]
        public List<AddEditSpecializationModel> passedSpecializations { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }

    public class CheckImport
    {
        public bool checker { get; set; }
        public string Errors { get; set; }
    }
}

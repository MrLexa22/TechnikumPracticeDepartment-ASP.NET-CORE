using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Models.ModelsStudentsPages
{
    public class ExportModelStudents
    {
        [Name("Email")]
        [Description("Email")]
        public string? Email { get; set; }

        [Name("Фамилия")]
        [Description("Фамилия")]
        public string? SurnameStudent { get; set; }

        [Name("Имя")]
        [Description("Имя")]
        public string? NameStudent { get; set; }

        [Name("Отчество (необязательно)")]
        [Description("Отчество (необязательно)")]
        public string? PatronymicNameStudent { get; set; }

        [Name("Группа")]
        [Description("Группа")]
        public string? GroupName { get; set; }

        [Name("Дата рождения (необязательно)")]
        [Description("Дата рождения (необязательно)")]
        public string? DateBirth { get; set; }

        [Name("Номер телефона (необязательно)")]
        [Description("Номер телефона (необязательно)")]
        public string? PhoneNumber { get; set; }
    }

    public class ImportFileStudents
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
        public List<AddEditStudent> notPassedStudents { get; set; }

        [ValidateNever]
        public List<AddEditStudent> passedStudents { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }
}

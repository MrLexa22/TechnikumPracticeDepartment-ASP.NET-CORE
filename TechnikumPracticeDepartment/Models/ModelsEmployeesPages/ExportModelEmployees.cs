using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Models.ModelsEmployeesPages
{
    public class ExportModelEmployees
    {
        [Name("Email")]
        [Description("Email")]
        public string Email { get; set; }

        [Name("Фамилия")]
        [Description("Фамилия")]
        public string SurnameUser { get; set; }

        [Name("Имя")]
        [Description("Имя")]
        public string NameUser { get; set; }

        [Name("Отчество")]
        [Description("Отчество")]
        public string? PatronymicNameUser { get; set; }

        [Name("Администратор")]
        [Description("Администратор")]
        public string Administrator { get; set; }

        [Name("Сотрудник производственного отдела")]
        [Description("Сотрудник производственного отдела")]
        public string Employee { get; set; }

        [Name("Доступность аккаунта")]
        [Description("Доступность аккаунта")]
        public string IsAvaliable { get; set; }
    }

    public class ImportFileEmployees
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
        public List<AddEditEmployee> notPassedEmployees { get; set; }

        [ValidateNever]
        public List<AddEditEmployee> passedEmployees { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }
}

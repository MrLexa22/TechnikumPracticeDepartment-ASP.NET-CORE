using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsEmployeesPages;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationsPages
{
    public class ExportModelOrganizations
    {
        [Name("Полное наименование организации")]
        [Description("Полное наименование организации")]
        public string FullNameOrganization { get; set; }

        [Name("Краткое наименование организации")]
        [Description("Краткое наименование организации")]
        public string NotFullNameOrganization { get; set; }

        [Name("Юридический адрес организации")]
        [Description("Юридический адрес организации")]
        public string AddressOrganization { get; set; }

        [Name("ИНН организации")]
        [Description("ИНН организации")]
        public string? InnOrganization { get; set; }

        [Name("Фамилия руководителя организации")]
        [Description("Фамилия руководителя организации")]
        public string SurnameContactOfOrganization { get; set; }

        [Name("Имя руководителя организации")]
        [Description("Имя руководителя организации")]
        public string NameContactOfOrganization { get; set; }

        [Name("Отчество руководителя организации")]
        [Description("Отчество руководителя организации")]
        public string? PatronymicContactOfOrganization { get; set; }

        [Name("Номер телефона руководителя организации")]
        [Description("Номер телефона руководителя организации")]
        public string? PhoneNumberContactOfOrganization { get; set; }

        [Name("Статус работы организации")]
        [Description("Статус работы организации")]
        public string? IsAvaliable { get; set; }
    }
    public class ImportFileOrganizations
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
        public List<AddEditOrganization> notPassedOrganizations { get; set; }

        [ValidateNever]
        public List<AddEditOrganization> passedOrganizations { get; set; }

        [ValidateNever]
        public List<CheckImport> errorsImport { get; set; }
    }
}

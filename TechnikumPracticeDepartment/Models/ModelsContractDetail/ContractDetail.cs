using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TechnikumPracticeDepartment.Models.ModelsContractDetail
{
    public class ContractDetail
    {
        [ValidateNever]
        public bool Error { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameDirector_IM { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени указана некорректно")]
        public string NamameDirector_IM { get; set; }

        [Remote("CheckPatronymicNameDirector_IM", "CotractDetails", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameDirector_IM { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Фамилия указана некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина фамилии указана некорректно")]
        public string SurnameDirector_ROD { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$", ErrorMessage = "Имя указано некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Длина имени указана некорректно")]
        public string NamameDirector_ROD { get; set; }

        [Remote("CheckPatronymicNameDirector_ROD", "CotractDetails", ErrorMessage = "Отчество указано некорректно")]
        public string? PatronymicNameDirector_ROD { get; set; }

        [Remote("CheckDovernDate", "CotractDetails", ErrorMessage = "Дата указана некорректно")]
        public DateTime DovernDate { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Длина номера доверенности указана некорректно")]
        public string NumberDovern { get; set; }

        [RegularExpression("^[0-9]{6},[А-Яа-яЁёa-zA-Z \\/.\\-\\\"«»,0-9()№#]*$", ErrorMessage = "Адрес организации указан некорректно (пример: 101000, г. Москва)")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Адрес организации указа некорректно")]
        public string AddressUniversity { get; set; }

        [RegularExpression("^[0-9]{10}", ErrorMessage = "ИНН указан некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string INN_University { get; set; }

        [RegularExpression("^[0-9]{9}", ErrorMessage = "КПП указан некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string KPP_University { get; set; }

        [RegularExpression("^[0-9]{8}", ErrorMessage = "ОКТМО указан некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string OKTMO_University { get; set; }

        [RegularExpression("^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ.,0-9 \\/.\\-\\\"«»]+)*$", ErrorMessage = "Имя банка некорректно")]
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Длина банка указана некорректно")]
        public string Bank_University { get; set; }
    }
}

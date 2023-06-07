using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsPracticeChartPages
{
    public class AddEditPracticeChart
    {
        [ValidateNever]
        public int ID_PracticeChart { get; set; }

        [ValidateNever]
        public string NamePractice { get; set; }
        public int SelectedIdPractice { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [StringLength(4, MinimumLength = 1, ErrorMessage = "Длина количества часов некорректна")]
        [Remote("CheckDoubleHours", "PracticeChart", ErrorMessage = "Количество часов должно быть указано в диапазоне от 2 до 2000. Пример: 108,5 (Можно только указать запятую)")]
        public string hours { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckDateStartCreate", "PracticeChart", AdditionalFields = "dateEndCreate", ErrorMessage = "Дата начала практики указана некорректна (больше даты окончания/указана больше 2-х месяцев назад/указано более 12 мяцев от текущей даты/дата окончания не указана)")]
        public DateTime dateStartCreate { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckDateEndCreate", "PracticeChart", AdditionalFields = "dateStartCreate", ErrorMessage = "Дата окончания практики указана некорректна (меньше даты начала/указана больше 2-х месяцев назад/указано более 12 мяцев от текущей даты)")]
        public DateTime dateEndCreate { get; set; }

        [ValidateNever]
        public List<Practice> list_practice { get; set; }

        [ValidateNever]
        public List<GroupsSpecialization> list_groups { get; set; }

        [ValidateNever]
        public List<DaysWithBool> list_days { get; set; }

        [ValidateNever]
        public List<PracticesChartDate> list_dates { get; set; }

        [ValidateNever]
        public bool IsEndedPractice { get; set; }
    }
    public class DaysWithBool
    {
        public string ShortNameDay { get; set; }
        public string NameDay { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AddEditPeriodPracticeChart
    {
        [ValidateNever]
        public int ChartPractice_ID { get; set; }

        [ValidateNever]
        public int ChartDates_ID { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckDateStart", "PracticeChart", AdditionalFields = "dateEnd, ChartDates_ID, ChartPractice_ID", ErrorMessage = "Дата начала практики указана некорректна:<br />Больше даты окончания <br />Или указана больше 2-х месяцев назад <br />Или указано более 12 мяцев от текущей даты <br />Или дата окончания не указана <br />Указанный период накладывается на уже созданный период")]
        public DateTime dateStart { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Remote("CheckDateEnd", "PracticeChart", AdditionalFields = "dateStart, ChartDates_ID, ChartPractice_ID", ErrorMessage = "Дата окончания практики указана некорректна:<br />Меньше даты начала <br />Или указана больше 2-х месяцев назад <br />Или указано более 12 мяцев от текущей даты <br />Указанный период накладывается на уже созданный период")]
        public DateTime dateEnd { get; set; }
    }
}

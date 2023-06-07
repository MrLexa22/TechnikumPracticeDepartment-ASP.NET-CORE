using TechnikumPracticeDepartment.Models.ModelsSpecializationPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsEmployeesPages
{
    public class IndexEmployeesModel
    {
        public List<User> list_employees { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Employees FilterViewModel { get; set; }
    }
}

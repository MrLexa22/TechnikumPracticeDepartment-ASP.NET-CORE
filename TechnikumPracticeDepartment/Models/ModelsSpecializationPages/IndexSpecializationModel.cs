using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsSpecializationPages
{
    public class IndexSpecializationModel
    {
        public List<Specialization> list_specialnosti { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Specialization FilterViewModel { get; set; }
    }
}

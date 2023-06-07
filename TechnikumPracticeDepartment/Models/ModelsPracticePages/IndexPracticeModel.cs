using TechnikumPracticeDepartment.Models.ModelsOrganizationsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsPracticePages
{
    public class IndexPracticeModel
    {
        public List<Practice> list_practice { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Practice FilterViewModel { get; set; }
    }
}

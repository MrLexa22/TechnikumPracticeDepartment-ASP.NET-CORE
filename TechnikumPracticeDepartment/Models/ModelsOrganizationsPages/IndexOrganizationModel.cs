using TechnikumPracticeDepartment.Models.ModelsGroupsPages;
using TechnikumPracticeDepartment.ModelsDB;

namespace TechnikumPracticeDepartment.Models.ModelsOrganizationsPages
{
    public class IndexOrganizationModel
    {
        public List<Organization> list_organization { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterViewModel_Organization FilterViewModel { get; set; }
    }
}
